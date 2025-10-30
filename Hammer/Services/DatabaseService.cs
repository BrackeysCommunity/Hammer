using Hammer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hammer.Services;

/// <summary>
///     Represents a service which connects to the Hammer database.
/// </summary>
internal sealed class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly IDbContextFactory<HammerContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="dbContextFactory">The <see cref="HammerContext" /> factory.</param>
    public DatabaseService(ILogger<DatabaseService> logger,
        IDbContextFactory<HammerContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Migrates the database from one source to another.
    /// </summary>
    public async Task<int> MigrateAsync(int batchSize = 1000, bool disableFkChecks = true)
    {
        await using HammerContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();

        if (!context.Database.IsMySql())
        {
            _logger.LogWarning("Cannot migrate from SQLite to SQLite. This operation will be skipped");
            return 0;
        }

        await using var migration = new HammerContext(
            new DbContextOptionsBuilder<HammerContext>()
                .UseSqlite("Data Source=data/hammer.db")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options);

        context.ChangeTracker.AutoDetectChangesEnabled = false;

        if (disableFkChecks)
        {
            await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=0;");
        }

        var totalInserted = 0;

        // Copy order: parents -> children (adjust to your model)
        totalInserted += await CopyAsync(migration.AltAccounts.AsNoTracking(), context.AltAccounts, context, batchSize);
        totalInserted += await CopyAsync(migration.BlockedReporters.AsNoTracking(), context.BlockedReporters, context, batchSize);
        totalInserted += await CopyAsync(migration.Rules.AsNoTracking(), context.Rules, context, batchSize);
        totalInserted += await CopyAsync(migration.MemberNotes.AsNoTracking(), context.MemberNotes, context, batchSize);
        totalInserted += await CopyAsync(migration.Mutes.AsNoTracking(), context.Mutes, context, batchSize);
        totalInserted += await CopyAsync(migration.TemporaryBans.AsNoTracking(), context.TemporaryBans, context, batchSize);
        totalInserted += await CopyAsync(migration.StaffMessages.AsNoTracking(), context.StaffMessages, context, batchSize);
        totalInserted += await CopyAsync(migration.ReportedMessages.AsNoTracking(), context.ReportedMessages, context, batchSize);
        totalInserted += await CopyAsync(migration.DeletedMessages.AsNoTracking(), context.DeletedMessages, context, batchSize);
        totalInserted += await CopyAsync(migration.Infractions.AsNoTracking(), context.Infractions, context, batchSize);
        totalInserted += await CopyAsync(migration.TrackedMessages.AsNoTracking(), context.TrackedMessages, context, batchSize);

        if (disableFkChecks)
        {
            await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=1;");
        }

        _logger.LogInformation("Migration complete. Inserted {Count} rows.", totalInserted);
        return totalInserted;
    }

    private static async Task<int> CopyAsync<T>(
        IQueryable<T> source,
        DbSet<T> dest,
        DbContext destCtx,
        int batchSize)
        where T : class
    {
        var inserted = 0;
        var buffer = new List<T>(batchSize);

        await foreach (T row in source.AsAsyncEnumerable())
        {
            buffer.Add(row);
            if (buffer.Count >= batchSize)
            {
                await dest.AddRangeAsync(buffer);
                inserted += buffer.Count;
                await destCtx.SaveChangesAsync();
                destCtx.ChangeTracker.Clear();
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
        {
            await dest.AddRangeAsync(buffer);
            inserted += buffer.Count;
            await destCtx.SaveChangesAsync();
            destCtx.ChangeTracker.Clear();
            buffer.Clear();
        }

        return inserted;
    }
}
