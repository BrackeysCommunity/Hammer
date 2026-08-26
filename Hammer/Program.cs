using Asp.Versioning;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Extensions;
using FluentResults.Extensions.AspNetCore;
using Hammer.Authentication;
using Hammer.Commands;
using Hammer.Commands.Infractions;
using Hammer.Commands.Notes;
using Hammer.Commands.Reports;
using Hammer.Commands.Rules;
using Hammer.Controllers;
using Hammer.Data;
using Hammer.Data.v5_compat;
using Hammer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using X10D.Hosting.DependencyInjection;

var workingDir = AppContext.BaseDirectory;

var dataDir = Path.Combine(workingDir, "data");
var logsDir = Path.Combine(workingDir, "logs");
Directory.CreateDirectory(dataDir);
Directory.CreateDirectory(logsDir);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logsDir, "latest.log"), rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddYamlFile(Path.Combine(dataDir, "config.yaml"), true, true);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc();

AspNetCoreResult.Setup(config => config.DefaultProfile = new HttpErrorResultEndpointProfile());
var apiToken = builder.Configuration.GetValue<string>("API_TOKEN") ?? throw new InvalidOperationException("API_TOKEN is not set");

builder.Services.AddAuthentication(ApiTokenDefaults.AuthenticationScheme)
    .AddScheme<ApiTokenAuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(
        ApiTokenDefaults.AuthenticationScheme, options => options.Token = apiToken);

builder.Services.AddAuthorization(options =>
{
    // require a valid API token on every endpoint by default; opt out with [AllowAnonymous]
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSingleton<ConfigurationService>();
const DiscordIntents intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents;
builder.Services.AddDiscordClient(Environment.GetEnvironmentVariable("DISCORD_TOKEN")!, intents);
builder.Services.ConfigureEventHandlers(events =>
{
    events.AddEventHandlers<BotService>(ServiceLifetime.Singleton);
    events.AddEventHandlers<DiscordLogService>(ServiceLifetime.Singleton);
    events.AddEventHandlers<InfractionService>(ServiceLifetime.Singleton);
    events.AddEventHandlers<MessageTrackingService>(ServiceLifetime.Singleton);
    events.AddEventHandlers<ModalResponseService>();
    events.AddEventHandlers<MuteService>(ServiceLifetime.Singleton);
    events.AddEventHandlers<StaffReactionService>();
    events.AddEventHandlers<UserReactionService>();
});

builder.Services.AddCommandsExtension((services, commands) =>
{
    var guilds = new List<ulong>();
    var configuration = services.GetRequiredService<IConfiguration>();

    foreach (var key in configuration.GetChildren().Select(s => s.Key))
    {
        if (ulong.TryParse(key, out var guildId))
        {
            guilds.Add(guildId);
        }
    }

    ulong[] guildIds = guilds.Count > 0 ? [.. guilds] : [];

    commands.AddCommands<AltCommand>(guildIds);
    commands.AddCommands<BadMessageCommand>(guildIds);
    commands.AddCommands<BanCommand>(guildIds);
    commands.AddCommands<DeleteMessageCommand>(guildIds);
    commands.AddCommands<GagCommand>(guildIds);
    commands.AddCommands<HistoryCommand>(guildIds);
    commands.AddCommands<InfractionCommand>(guildIds);
    commands.AddCommands<InfoCommand>(guildIds);
    commands.AddCommands<KickCommand>(guildIds);
    commands.AddCommands<MessageCommand>(guildIds);
    commands.AddCommands<MessageHistoryCommand>(guildIds);
    commands.AddCommands<MigrateDatabaseCommand>(guildIds);
    commands.AddCommands<MuteCommand>(guildIds);
    commands.AddCommands<NoteCommand>(guildIds);
    commands.AddCommands<PruneInfractionsCommand>(guildIds);
    commands.AddCommands<ReportCommands>(guildIds);
    commands.AddCommands<RuleCommand>(guildIds);
    commands.AddCommands<RulesCommand>(guildIds);
    commands.AddCommands<SelfHistoryCommand>(guildIds);
    commands.AddCommands<StaffHistoryCommand>(guildIds);
    commands.AddCommands<UnbanCommand>(guildIds);
    commands.AddCommands<UnmuteCommand>(guildIds);
    commands.AddCommands<UserInfoCommand>(guildIds);
    commands.AddCommands<ViewInfractionCommand>(guildIds);
    commands.AddCommands<ViewMessageCommand>(guildIds);
    commands.AddCommands<WarnCommand>(guildIds);
});

builder.Services.AddDbContextFactory<V5Context>();
builder.Services.AddDbContextFactory<HammerContext>((services, options) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<HammerContext>>();
    var connectionString = configuration.GetValue<string>("DB_CONNECTION_STRING") ??
                           throw new InvalidOperationException("DB_CONNECTION_STRING is not set");

    logger.LogTrace("Using PostgreSQL database provider for HammerContext");
    HammerContextConfig.Configure(options, connectionString);
});

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<InfractionStatisticsService>();
builder.Services.AddSingleton<MailmanService>();
builder.Services.AddSingleton<MemberNoteService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<MessageDeletionService>();
builder.Services.AddSingleton<WarningService>();

builder.Services.AddHostedSingleton<AltAccountService>();
builder.Services.AddHostedSingleton<BanService>();
builder.Services.AddHostedSingleton<InfractionCooldownService>();
builder.Services.AddHostedSingleton<MessageReportService>();
builder.Services.AddHostedSingleton<MuteService>();
builder.Services.AddHostedSingleton<RuleService>();

builder.Services.AddHostedSingleton<BotService>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await ConfigureMigrationsAsync<HammerContext>(app.Services);
await app.RunAsync();
return;

async Task ConfigureMigrationsAsync<TContext>(IServiceProvider services) where TContext : DbContext
{
    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();
    await using var context = await factory.CreateDbContextAsync();

    for (var attempt = 1;; attempt++)
    {
        var contextName = typeof(TContext).Name;

        try
        {
            string[] pending = [.. await context.Database.GetPendingMigrationsAsync()];
            if (pending.Length > 0)
            {
                logger.LogInformation("Applying migrations for {Context}: {Migrations}", contextName, string.Join(", ", pending));
                await context.Database.MigrateAsync();
            }

            break;
        }
        catch (Exception ex) when (attempt < 5)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt} for {Context} failed. Retrying...", attempt, contextName);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
