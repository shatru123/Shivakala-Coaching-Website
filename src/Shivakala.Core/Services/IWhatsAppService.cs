namespace Shivakala.Core.Services;

public interface IWhatsAppService
{
    string ActiveProvider { get; }
    bool IsConfigured { get; }
    bool IsAuthenticated { get; }

    Task<byte[]?> GetQrCodeAsync(CancellationToken ct = default);
    Task<bool> CheckStatusAsync(CancellationToken ct = default);
    Task<bool> SendMessageAsync(string mobile, string message, CancellationToken ct = default);
    Task<int> BroadcastAsync(IEnumerable<string> mobiles, string message, CancellationToken ct = default);
    Task<bool> DisconnectAsync(CancellationToken ct = default);

    string GenerateWhatsAppWebLink(string mobile, string message);
}
