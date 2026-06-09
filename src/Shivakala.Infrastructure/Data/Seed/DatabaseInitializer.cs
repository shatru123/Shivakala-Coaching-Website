using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using Npgsql;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Configuration;
using Shivakala.Infrastructure.Services;

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
            var canSeedContentSettings = true;

            // ── Step 1: detect broken migration (marked applied but column missing) ──
            if (db.Database.IsSqlite())
                await FixSchemaDriftAsync(db, logger);

            // ── Step 2: apply any pending migrations ─────────────────────────────
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count > 0)
            {
                try
                {
                    logger?.LogInformation("Applying {Count} pending migration(s): {Names}",
                        pending.Count, string.Join(", ", pending));
                    await db.Database.MigrateAsync();
                    logger?.LogInformation("✅  All migrations applied successfully.");
                }
                catch (Exception ex) when (db.Database.IsSqlServer())
                {
                    canSeedContentSettings = false;
                    logger?.LogError(ex,
                        "⚠️  SQL Server migrations could not be applied automatically during startup. " +
                        "The app will continue with built-in public-site defaults until the database schema is updated.");
                }
            }
            else
            {
                logger?.LogInformation("✅  Database schema is already up to date.");
            }

            if (canSeedContentSettings)
            {
                try
                {
                    if (!await db.HomePageSectionSettings.AnyAsync())
                    {
                        db.HomePageSectionSettings.Add(new Core.Entities.HomePageSectionSettings());
                        await db.SaveChangesAsync();
                        logger?.LogInformation("✅  Homepage content settings created.");
                    }

                    var homeSettings = await db.HomePageSectionSettings.FirstOrDefaultAsync();
                    if (homeSettings is not null)
                    {
                        var updated = false;
                        if (string.IsNullOrWhiteSpace(homeSettings.HeroBannerImageUrl)) { homeSettings.HeroBannerImageUrl = "/img/Banner.jpeg"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.HeroBannerAltText)) { homeSettings.HeroBannerAltText = "Shivakala Classes admissions banner"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingEyebrow)) { homeSettings.TrendingEyebrow = "Trending Now"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingEyebrowMarathi)) { homeSettings.TrendingEyebrowMarathi = "नवीन अपडेट"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingTitle)) { homeSettings.TrendingTitle = "Admissions open for the new academic year"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingTitleMarathi)) { homeSettings.TrendingTitleMarathi = "नवीन शैक्षणिक वर्षासाठी प्रवेश सुरू"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingDescription)) { homeSettings.TrendingDescription = "Highlight important announcements, batches, offers, or events right from the admin panel."; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingDescriptionMarathi)) { homeSettings.TrendingDescriptionMarathi = "महत्त्वाच्या घोषणा, बॅचेस, ऑफर्स किंवा इव्हेंट्स अॅडमिन पॅनलमधून लगेच दाखवा."; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingImageUrl)) { homeSettings.TrendingImageUrl = "/img/Banner.jpeg"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingAltText)) { homeSettings.TrendingAltText = "Trending banner for Shivakala Coaching Classes"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingLinkText)) { homeSettings.TrendingLinkText = "Explore Now"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingLinkTextMarathi)) { homeSettings.TrendingLinkTextMarathi = "अधिक जाणून घ्या"; updated = true; }
                        if (string.IsNullOrWhiteSpace(homeSettings.TrendingLinkUrl)) { homeSettings.TrendingLinkUrl = "/registration"; updated = true; }
                        if (updated)
                        {
                            await db.SaveChangesAsync();
                            logger?.LogInformation("✅  Homepage banner defaults normalized.");
                        }
                    }
                }
                catch (Exception ex) when (db.Database.IsSqlServer())
                {
                    logger?.LogWarning(ex,
                        "Homepage content settings are unavailable because the SQL Server schema is behind the deployed code. " +
                        "Public pages will use in-code defaults until migrations are applied.");
                }

                try
                {
                    if (!await db.AboutPageSectionSettings.AnyAsync())
                    {
                        db.AboutPageSectionSettings.Add(new Core.Entities.AboutPageSectionSettings());
                        await db.SaveChangesAsync();
                        logger?.LogInformation("✅  About page content settings created.");
                    }
                }
                catch (Exception ex) when (db.Database.IsSqlServer())
                {
                    logger?.LogWarning(ex,
                        "About page content settings are unavailable because the SQL Server schema is behind the deployed code.");
                }
            }

            // ── Step 3: ensure teacher/parent portal accounts exist ───────────────
            var portalUsers = scope.ServiceProvider.GetRequiredService<IPortalUserService>();
            var adminCredentials = scope.ServiceProvider
                .GetRequiredService<IOptions<AdminCredentialsOptions>>()
                .Value;
            await portalUsers.EnsureAdminAccountAsync(adminCredentials.Username, adminCredentials.Password);
            await portalUsers.SyncMissingPortalAccountsAsync();
            logger?.LogInformation("✅  Portal user accounts synced.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌  Database initialization failed: {Msg}", BuildFriendlyErrorMessage(db, ex));
            throw;
        }
    }

    private static string BuildFriendlyErrorMessage(ShivakalaDbContext db, Exception ex)
    {
        if (db.Database.IsSqlite())
            return $"{ex.Message}. If the SQLite file is corrupted, delete App_Data/shivakala.db and restart.";

        if (db.Database.IsNpgsql())
        {
            if (ex is NpgsqlException or System.Net.Sockets.SocketException)
            {
                var connectionString = db.Database.GetConnectionString() ?? "";
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                return $"Could not connect to PostgreSQL at {builder.Host}:{builder.Port}/{builder.Database}. " +
                       $"Start PostgreSQL, verify the connection string, or switch `Database:Provider` back to `Sqlite` for local development.";
            }

            return $"{ex.Message}. Verify PostgreSQL connection settings or switch `Database:Provider` to `Sqlite` for local development.";
        }

        if (db.Database.IsSqlServer())
        {
            if (ex is SqlException or System.Net.Sockets.SocketException)
            {
                var connectionString = db.Database.GetConnectionString() ?? "";
                var builder = new SqlConnectionStringBuilder(connectionString);
                return $"Could not connect to SQL Server at {builder.DataSource} / database '{builder.InitialCatalog}'. " +
                       $"Verify the SQL Server connection string, firewall rules, and host database settings.";
            }

            return $"{ex.Message}. Verify SQL Server connection settings or switch `Database:Provider` to `Sqlite` for local development.";
        }

        return ex.Message;
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
