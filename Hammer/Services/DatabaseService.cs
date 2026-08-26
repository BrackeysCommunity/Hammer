using Hammer.Data;
using Hammer.Data.v5_compat;
using Microsoft.EntityFrameworkCore;

namespace Hammer.Services;

/// <summary>
///     Represents a service which connects to the Hammer database.
/// </summary>
internal sealed class DatabaseService
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly IDbContextFactory<HammerContext> _dbContextFactory;
    private readonly IDbContextFactory<V5Context> _migrationContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="dbContextFactory">The <see cref="HammerContext" /> factory.</param>
    /// <param name="migrationContextFactory">The <see cref="V5Context" /> factory.</param>
    public DatabaseService(ILogger<DatabaseService> logger,
        IDbContextFactory<HammerContext> dbContextFactory,
        IDbContextFactory<V5Context> migrationContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _migrationContextFactory = migrationContextFactory;
    }

    /// <summary>
    ///     Migrates the database from one source to another.
    /// </summary>
    /// <param name="batchSize">The number of rows to insert in each batch.</param>
    public async Task<int> MigrateAsync(int batchSize = 1000)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();

        await using var migration = await _migrationContextFactory.CreateDbContextAsync();
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var totalInserted = 0;

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
