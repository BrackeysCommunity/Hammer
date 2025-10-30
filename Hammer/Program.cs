using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Extensions;
using Hammer.Commands;
using Hammer.Commands.Infractions;
using Hammer.Commands.Notes;
using Hammer.Commands.Reports;
using Hammer.Commands.Rules;
using Hammer.Configuration;
using Hammer.Data;
using Hammer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Serilog;
using X10D.Hosting.DependencyInjection;

Directory.CreateDirectory("data");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddYamlFile("data/config.yaml", true, true);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddSingleton<ConfigurationService>();
const DiscordIntents intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents;
builder.Services.AddDiscordClient(Environment.GetEnvironmentVariable("DISCORD_TOKEN")!, intents);
builder.Services.ConfigureEventHandlers(events =>
{
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

builder.Services.AddDbContextFactory<HammerContext>((services, optionsBuilder) =>
{
    ILogger<HammerContext> logger = services.GetRequiredService<ILogger<HammerContext>>();
    ConfigurationService configurationService = services.GetRequiredService<ConfigurationService>();
    DatabaseConfiguration databaseConfiguration = configurationService.BotConfiguration.Database;
    switch (databaseConfiguration.Provider)
    {
        case "mysql":
            logger.LogTrace("Using MySQL/MariaDB database provider");
            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = databaseConfiguration.Host,
                Port = (uint)(databaseConfiguration.Port ?? 3306),
                Database = databaseConfiguration.Database,
                UserID = databaseConfiguration.Username,
                Password = databaseConfiguration.Password
            };

            var connectionString = connectionStringBuilder.ToString();
            var version = ServerVersion.AutoDetect(connectionString);

            logger.LogTrace("Server version is {Version}", version);
            optionsBuilder.UseMySql(connectionString, version);
            break;

        default:
            logger.LogTrace("Using SQLite database provider");
            optionsBuilder.UseSqlite("Data Source='data/hammer.db'");
            break;
    }
});

using (IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope())
{
    ILogger<HammerContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<HammerContext>>();
    IDbContextFactory<HammerContext> factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<HammerContext>>();
    await using HammerContext context = await factory.CreateDbContextAsync();

    for (var attempt = 1;; attempt++)
    {
        try
        {
            if (context.Database.IsMySql())
            {
                string[] pending = (await context.Database.GetPendingMigrationsAsync()).ToArray();
                if (pending.Length > 0)
                {
                    logger.LogInformation("Applying migrations: {Migrations}", string.Join(", ", pending));
                    await context.Database.MigrateAsync();
                }
            }
            else
            {
                // Dev convenience: create SQLite schema without migrations
                await context.Database.EnsureCreatedAsync();
            }

            break;
        }
        catch (Exception ex) when (attempt < 5)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt} failed. Retrying...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

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

IHost app = builder.Build();
await app.RunAsync();
