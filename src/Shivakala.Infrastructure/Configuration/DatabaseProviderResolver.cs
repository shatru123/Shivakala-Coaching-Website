namespace Shivakala.Infrastructure.Configuration;

public static class DatabaseProviderResolver
{
    public static string Normalize(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
            return DatabaseProviderNames.Sqlite;

        return provider.Trim().ToLowerInvariant() switch
        {
            "sqlite" => DatabaseProviderNames.Sqlite,
            "postgres" or "postgresql" or "npgsql" => DatabaseProviderNames.PostgreSql,
            _ => throw new InvalidOperationException(
                $"Unsupported database provider '{provider}'. Use '{DatabaseProviderNames.Sqlite}' or '{DatabaseProviderNames.PostgreSql}'.")
        };
    }

    public static bool IsSqlite(string? provider) => Normalize(provider) == DatabaseProviderNames.Sqlite;

    public static bool IsPostgreSql(string? provider) => Normalize(provider) == DatabaseProviderNames.PostgreSql;
}
