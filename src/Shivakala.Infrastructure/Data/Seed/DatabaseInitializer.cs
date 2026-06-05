using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Configuration;

namespace Shivakala.Infrastructure.Data.Seed;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db     = scope.ServiceProvider.GetRequiredService<ShivakalaDbContext>();
        var logger = scope.ServiceProvider
                         .GetService<ILoggerFactory>()
                         ?.CreateLogger("DatabaseInitializer");

        // Ensure App_Data directory exists (SQLite file goes here)
        var cs = db.Database.GetConnectionString() ?? "";
        if (db.Database.IsSqlite() && cs.Contains("App_Data", StringComparison.OrdinalIgnoreCase))
            Directory.CreateDirectory("App_Data");

        try
        {
            // ── Step 1: detect broken migration (marked applied but column missing) ──
            if (db.Database.IsSqlite())
                await FixSchemaDriftAsync(db, logger);

            // ── Step 2: apply any pending migrations ─────────────────────────────
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count > 0)
            {
                logger?.LogInformation("Applying {Count} pending migration(s): {Names}",
                    pending.Count, string.Join(", ", pending));
                await db.Database.MigrateAsync();
                logger?.LogInformation("✅  All migrations applied successfully.");
            }
            else
            {
                logger?.LogInformation("✅  Database schema is already up to date.");
            }

            if (!await db.HomePageSectionSettings.AnyAsync())
            {
                db.HomePageSectionSettings.Add(new Core.Entities.HomePageSectionSettings());
                await db.SaveChangesAsync();
                logger?.LogInformation("✅  Homepage content settings created.");
            }

            // ── Step 3: ensure teacher/parent portal accounts exist ───────────────
            var portalUsers = scope.ServiceProvider.GetRequiredService<IPortalUserService>();
            await portalUsers.SyncMissingPortalAccountsAsync();
            logger?.LogInformation("✅  Portal user accounts synced.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "❌  Database initialization failed: {Msg}. " +
                "If you keep seeing this, delete App_Data/shivakala.db and restart.", ex.Message);
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Detects the common case where the EF migrations history table records a
    // migration as "applied" but the DDL never actually ran (e.g. because the
    // migration class was missing the [DbContext] attribute on a previous push).
    // Fix: delete the bad history entry so MigrateAsync() re-applies it.
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task FixSchemaDriftAsync(ShivakalaDbContext db, ILogger? logger)
    {
        const string migrationId = "20260602000000_AddManagementSystem";

        // If the history table itself doesn't exist, it's a fresh DB — nothing to fix.
        bool historyExists;
        try
        {
            _ = await db.Database.GetAppliedMigrationsAsync();
            historyExists = true;
        }
        catch
        {
            historyExists = false;
        }

        if (!historyExists) return;

        // Check whether this specific migration is recorded as applied
        var applied = (await db.Database.GetAppliedMigrationsAsync()).ToHashSet();
        if (!applied.Contains(migrationId)) return; // not recorded → MigrateAsync will handle it

        // Migration IS recorded — verify the actual schema
        var columnOk = await ColumnExistsAsync(db, "Students", "AdmissionNumber");
        if (columnOk) return; // schema looks correct

        // Schema is broken: column is missing despite migration being recorded.
        // Remove the history entry so MigrateAsync re-applies the migration.
        logger?.LogWarning(
            "⚠️  Schema drift detected: '{Migration}' is in migration history " +
            "but 'Students.AdmissionNumber' column is missing. " +
            "Removing stale history entry and re-applying migration...", migrationId);

        await db.Database.ExecuteSqlRawAsync(
            $"DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '{migrationId}'");

        logger?.LogInformation("Stale history entry removed. Migration will be re-applied now.");
    }

    private static async Task<bool> ColumnExistsAsync(ShivakalaDbContext db, string table, string column)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            var needsOpen = conn.State != System.Data.ConnectionState.Open;
            if (needsOpen) await conn.OpenAsync();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText =
                    $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='{column}'";
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt64(result ?? 0) > 0;
            }
            finally
            {
                if (needsOpen) await conn.CloseAsync();
            }
        }
        catch
        {
            return false; // table or DB doesn't exist → fresh DB
        }
    }
}
