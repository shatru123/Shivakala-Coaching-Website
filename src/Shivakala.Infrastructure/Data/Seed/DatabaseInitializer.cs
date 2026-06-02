using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        try
        {
            // Ensure App_Data folder exists
            var connStr = db.Database.GetConnectionString() ?? "";
            if (connStr.Contains("App_Data"))
                Directory.CreateDirectory("App_Data");

            // Apply all pending migrations (creates DB if it doesn't exist)
            var pending = await db.Database.GetPendingMigrationsAsync();
            var pendingList = pending.ToList();

            if (pendingList.Count > 0)
            {
                logger?.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                    pendingList.Count, string.Join(", ", pendingList));

                await db.Database.MigrateAsync();

                logger?.LogInformation("All migrations applied successfully.");
            }
            else
            {
                logger?.LogInformation("Database is up to date. No pending migrations.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "Database migration failed. " +
                "If this is a schema conflict, delete App_Data/shivakala.db and restart. " +
                "Error: {Message}", ex.Message);

            // Last-resort fallback: ensure schema exists even if migrations fail
            // (safe for development; do NOT use EnsureCreated in production with migrations)
            try
            {
                await db.Database.EnsureCreatedAsync();
                logger?.LogWarning("Used EnsureCreated as fallback — migration history may be incomplete.");
            }
            catch (Exception fallbackEx)
            {
                logger?.LogError(fallbackEx, "EnsureCreated fallback also failed.");
                throw; // Re-throw original so startup fails visibly
            }
        }
    }
}
