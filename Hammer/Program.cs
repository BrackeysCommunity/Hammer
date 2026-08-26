using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Extensions;
using Hammer.Commands;
using Hammer.Commands.Infractions;
using Hammer.Commands.Notes;
using Hammer.Commands.Reports;
using Hammer.Commands.Rules;
using Hammer.Data;
using Hammer.Services;
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
builder.Configuration.AddYamlFile(Path.Combine(dataDir, "config.yaml"), true, true);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

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

builder.Services.AddCommandsExtension((_, commands) =>
{
    commands.AddCommands<AltCommand>();
    commands.AddCommands<BadMessageCommand>();
    commands.AddCommands<BanCommand>();
    commands.AddCommands<DeleteMessageCommand>();
    commands.AddCommands<GagCommand>();
    commands.AddCommands<HistoryCommand>();
    commands.AddCommands<InfractionCommand>();
    commands.AddCommands<InfoCommand>();
    commands.AddCommands<KickCommand>();
    commands.AddCommands<MessageCommand>();
    commands.AddCommands<MessageHistoryCommand>();
    commands.AddCommands<MigrateDatabaseCommand>();
    commands.AddCommands<MuteCommand>();
    commands.AddCommands<NoteCommand>();
    commands.AddCommands<PruneInfractionsCommand>();
    commands.AddCommands<ReportCommands>();
    commands.AddCommands<RuleCommand>();
    commands.AddCommands<RulesCommand>();
    commands.AddCommands<SelfHistoryCommand>();
    commands.AddCommands<StaffHistoryCommand>();
    commands.AddCommands<UnbanCommand>();
    commands.AddCommands<UnmuteCommand>();
    commands.AddCommands<UserInfoCommand>();
    commands.AddCommands<ViewInfractionCommand>();
    commands.AddCommands<ViewMessageCommand>();
    commands.AddCommands<WarnCommand>();
});

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
