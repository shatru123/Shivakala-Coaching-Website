namespace Shivakala.Infrastructure.Configuration;

public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    /// <summary>
    /// Provider selection: "CloudApi" (Official Meta WhatsApp API), "Sidecar" (Node.js sidecar), or "DirectLink".
    /// Defaults to "CloudApi".
    /// </summary>
    public string Provider { get; set; } = "CloudApi";

    // Meta WhatsApp Cloud API settings
    public string? PhoneNumberId { get; set; }
    public string? AccessToken { get; set; }
    public string? BusinessAccountId { get; set; }

    // Legacy Node.js sidecar settings
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
}
