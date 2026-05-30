namespace Shivakala.Infrastructure.Services;

public sealed class AdminCredentialsOptions
{
    public const string SectionName = "AdminCredentials";

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "P@$$w0rd";
}
