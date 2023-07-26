using Hammer.Configuration;
using Hammer.Data.EntityConfigurations;
using Hammer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MuteConfiguration = Hammer.Data.EntityConfigurations.MuteConfiguration;

namespace Hammer.Data;

/// <summary>
///     Represents a session with the <c>hammer.db</c> database.
/// </summary>
internal sealed class HammerContext : DbContext
{
    private readonly ILogger<HammerContext> _logger;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HammerContext" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationService">The configuration service.</param>
    public HammerContext(ILogger<HammerContext> logger, ConfigurationService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;
    }

    /// <summary>
    ///     Gets the set of alt accounts.
    /// </summary>
    /// <value>The set of alt accounts.</value>
    public DbSet<AltAccount> AltAccounts { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of users who are blocked from making reports.
    /// </summary>
    /// <value>The set of blocked reporters.</value>
    public DbSet<BlockedReporter> BlockedReporters { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of staff-deleted messages.
    /// </summary>
    /// <value>The set of staff-deleted messages.</value>
    public DbSet<DeletedMessage> DeletedMessages { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of infractions.
    /// </summary>
    /// <value>The set of infractions.</value>
    public DbSet<Infraction> Infractions { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of member notes.
    /// </summary>
    /// <value>The set of member notes.</value>
    public DbSet<MemberNote> MemberNotes { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of mutes.
    /// </summary>
    /// <value>The set of mutes.</value>
    public DbSet<Mute> Mutes { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of reported messages.
    /// </summary>
    /// <value>The set of reported messages.</value>
    public DbSet<ReportedMessage> ReportedMessages { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of rules.
    /// </summary>
    /// <value>The set of rules.</value>
    public DbSet<Rule> Rules { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of staff messages.
    /// </summary>
    /// <value>The set of staff messages.</value>
    public DbSet<StaffMessage> StaffMessages { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of temporary bans.
    /// </summary>
    /// <value>The set of temporary bans.</value>
    public DbSet<TemporaryBan> TemporaryBans { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of tracked messages.
    /// </summary>
    /// <value>The set of tracked messages.</value>
    public DbSet<TrackedMessage> TrackedMessages { get; private set; } = null!;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        DatabaseConfiguration databaseConfiguration = _configurationService.BotConfiguration.Database;
        switch (databaseConfiguration.Provider)
        {
            case "sqlite":
                _logger.LogTrace("Using SQLite database provider");
                optionsBuilder.UseSqlite("Data Source='data/hammer.db'");
                break;

            default:
                throw new InvalidOperationException("Invalid database provider.");
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AltAccountConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new BlockedReporterConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new DeletedMessageConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new InfractionConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new MemberNoteConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new MuteConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new StaffMessageConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new ReportedMessageConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new TemporaryBanConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new TrackedMessageConfiguration(_configurationService));
        modelBuilder.ApplyConfiguration(new RuleConfiguration(_configurationService));
    }
}
