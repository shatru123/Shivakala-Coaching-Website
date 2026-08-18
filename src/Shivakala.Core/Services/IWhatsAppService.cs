namespace Shivakala.Core.Services;

public interface IWhatsAppService
{
    /// <summary>Returns QR code PNG bytes for scanning.</summary>
    Task<byte[]?> GetQrCodeAsync(CancellationToken ct = default);
    Task<bool> CheckStatusAsync(CancellationToken ct = default);
    bool IsAuthenticated { get; }
    Task<bool> SendMessageAsync(string mobile, string message, CancellationToken ct = default);
    Task<int> BroadcastAsync(IEnumerable<string> mobiles, string message, CancellationToken ct = default);
    Task<bool> DisconnectAsync(CancellationToken ct = default);
}
