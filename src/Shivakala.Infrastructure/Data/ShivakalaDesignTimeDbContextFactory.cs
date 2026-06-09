using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shivakala.Infrastructure.Configuration;

namespace Shivakala.Infrastructure.Data;

public sealed class ShivakalaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShivakalaDbContext>
{
    public ShivakalaDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var providerArg = args.FirstOrDefault(arg => arg.StartsWith("--provider=", StringComparison.OrdinalIgnoreCase));
        var providerValue = providerArg?.Split('=', 2)[1]
            ?? configuration[$"{DatabaseOptions.SectionName}:Provider"]
            ?? Environment.GetEnvironmentVariable("SHIVAKALA_DB_PROVIDER")
            ?? DatabaseProviderNames.Sqlite;

        var provider = DatabaseProviderResolver.Normalize(providerValue);
        var builder = new DbContextOptionsBuilder<ShivakalaDbContext>();

        if (DatabaseProviderResolver.IsPostgreSql(provider))
        {
            var postgresConnection = configuration.GetConnectionString("PostgreSql")
                ?? configuration["DATABASE_URL"]
                ?? "Host=localhost;Port=5432;Database=shivakala;Username=postgres;Password=postgres";
            builder.UseNpgsql(postgresConnection,
                sql => sql.MigrationsAssembly("Shivakala.PostgresMigrations"));
        }
        else if (DatabaseProviderResolver.IsSqlServer(provider))
        {
            var sqlServerConnection = configuration.GetConnectionString("SqlServer")
                ?? "Server=localhost,14333;Database=shivakala;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";
            builder.UseSqlServer(sqlServerConnection,
                sql => sql.MigrationsAssembly("Shivakala.SqlServerMigrations"));
        }
        else
        {
            var sqliteConnection = configuration.GetConnectionString("Sqlite")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=App_Data/shivakala.db";
            builder.UseSqlite(sqliteConnection,
                sql => sql.MigrationsAssembly("Shivakala.Infrastructure"));
        }

        return new ShivakalaDbContext(builder.Options);
    }
}
