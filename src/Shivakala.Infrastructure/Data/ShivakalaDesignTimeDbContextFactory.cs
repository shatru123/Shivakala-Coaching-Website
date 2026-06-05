using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shivakala.Infrastructure.Configuration;

namespace Shivakala.Infrastructure.Data;

public sealed class ShivakalaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShivakalaDbContext>
{
    public ShivakalaDbContext CreateDbContext(string[] args)
    {
        var providerArg = args.FirstOrDefault(arg => arg.StartsWith("--provider=", StringComparison.OrdinalIgnoreCase));
        var providerValue = providerArg?.Split('=', 2)[1]
            ?? Environment.GetEnvironmentVariable("SHIVAKALA_DB_PROVIDER")
            ?? DatabaseProviderNames.Sqlite;

        var provider = DatabaseProviderResolver.Normalize(providerValue);
        var builder = new DbContextOptionsBuilder<ShivakalaDbContext>();

        if (DatabaseProviderResolver.IsPostgreSql(provider))
        {
            const string postgresConnection = "Host=localhost;Port=5432;Database=shivakala;Username=postgres;Password=postgres";
            builder.UseNpgsql(postgresConnection,
                sql => sql.MigrationsAssembly("Shivakala.PostgresMigrations"));
        }
        else if (DatabaseProviderResolver.IsSqlServer(provider))
        {
            const string sqlServerConnection = "Server=localhost,14333;Database=shivakala;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";
            builder.UseSqlServer(sqlServerConnection,
                sql => sql.MigrationsAssembly("Shivakala.SqlServerMigrations"));
        }
        else
        {
            const string sqliteConnection = "Data Source=App_Data/shivakala.db";
            builder.UseSqlite(sqliteConnection,
                sql => sql.MigrationsAssembly("Shivakala.Infrastructure"));
        }

        return new ShivakalaDbContext(builder.Options);
    }
}
