namespace Shivakala.Infrastructure.Configuration;

public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    public string? BaseUrl { get; set; }

    public string? ApiKey { get; set; }
}
