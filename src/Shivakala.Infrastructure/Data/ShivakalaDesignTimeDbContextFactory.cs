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
        
        // Resolve configuration base path: check current directory or src/Shivakala.Web
        var currentDir = Directory.GetCurrentDirectory();
        var basePath = currentDir;
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            var webPath = Path.Combine(currentDir, "src", "Shivakala.Web");
            if (File.Exists(Path.Combine(webPath, "appsettings.json")))
            {
                basePath = webPath;
            }
            else
            {
                var parentWebPath = Path.Combine(currentDir, "..", "Shivakala.Web");
                if (File.Exists(Path.Combine(parentWebPath, "appsettings.json")))
                {
                    basePath = Path.GetFullPath(parentWebPath);
                }
            }
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Check command line arguments for provider or connection
        var providerArg = args.FirstOrDefault(arg => arg.StartsWith("--provider=", StringComparison.OrdinalIgnoreCase));
        var connectionArg = args.FirstOrDefault(arg => arg.StartsWith("--connection=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1]
            ?? (args.Contains("--connection", StringComparer.OrdinalIgnoreCase)
                ? args.SkipWhile(a => !a.Equals("--connection", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault()
                : null);

        var providerValue = providerArg?.Split('=', 2)[1]
            ?? Environment.GetEnvironmentVariable("SHIVAKALA_DB_PROVIDER")
            ?? Environment.GetEnvironmentVariable("Database__Provider")
            ?? configuration[$"{DatabaseOptions.SectionName}:Provider"]
            ?? DatabaseProviderNames.Sqlite;

        var provider = DatabaseProviderResolver.Normalize(providerValue);
        var builder = new DbContextOptionsBuilder<ShivakalaDbContext>();

        if (DatabaseProviderResolver.IsPostgreSql(provider))
        {
            var postgresConnection = connectionArg
                ?? configuration.GetConnectionString("PostgreSql")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
                ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? configuration["DATABASE_URL"]
                ?? "Host=localhost;Port=5432;Database=shivakala;Username=postgres;Password=postgres";
            builder.UseNpgsql(postgresConnection,
                sql => sql.MigrationsAssembly("Shivakala.PostgresMigrations"));
        }
        else if (DatabaseProviderResolver.IsSqlServer(provider))
        {
            var sqlServerConnection = connectionArg
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer")
                ?? Environment.GetEnvironmentVariable("PROD_SQLSERVER_CONNECTION_STRING")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings:SqlServer")
                ?? configuration.GetConnectionString("SqlServer")
                ?? (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : "Server=localhost,14333;Database=shivakala;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true");

            if (string.IsNullOrWhiteSpace(sqlServerConnection))
            {
                throw new InvalidOperationException(
                    "SQL Server connection string was not found. Please specify it via '--connection', 'ConnectionStrings__SqlServer', or 'PROD_SQLSERVER_CONNECTION_STRING'.");
            }

            builder.UseSqlServer(sqlServerConnection,
                sql => sql.MigrationsAssembly("Shivakala.SqlServerMigrations"));
        }
        else
        {
            var sqliteConnection = connectionArg
                ?? configuration.GetConnectionString("Sqlite")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__Sqlite")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=App_Data/shivakala.db";
            builder.UseSqlite(sqliteConnection,
                sql => sql.MigrationsAssembly("Shivakala.Infrastructure"));
        }

        return new ShivakalaDbContext(builder.Options);
    }
}

