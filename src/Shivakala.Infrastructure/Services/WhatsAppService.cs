using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Shivakala.Core.Services;

namespace Shivakala.Infrastructure.Services;

/// <summary>
/// Bridges to the Node.js whatsapp-web.js sidecar running on localhost:3500.
/// The sidecar is started separately (see /whatsapp-sidecar/README.md).
/// Falls back gracefully when the sidecar is not running.
/// </summary>
public sealed class WhatsAppService : IWhatsAppService, IDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger<WhatsAppService> _logger;
    private bool _authenticated;

    public bool IsAuthenticated => _authenticated;

    public WhatsAppService(ILogger<WhatsAppService> logger)
    {
        _logger = logger;
        _http = new HttpClient { BaseAddress = new Uri("http://localhost:3500"), Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<byte[]?> GetQrCodeAsync(CancellationToken ct)
    {
        try
        {
            var resp = await _http.GetAsync("/qr", ct);
            if (!resp.IsSuccessStatusCode) return null;
            var payload = await resp.Content.ReadFromJsonAsync<QrPayload>(ct);
            if (payload?.Authenticated == true) { _authenticated = true; return null; }
            if (string.IsNullOrWhiteSpace(payload?.QrBase64)) return null;
            return Convert.FromBase64String(payload.QrBase64.Split(',').Last());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp sidecar unreachable — QR fetch failed");
            return null;
        }
    }

    public async Task<bool> SendMessageAsync(string mobile, string message, CancellationToken ct)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/send", new { mobile, message }, ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp send failed to {Mobile}", mobile);
            return false;
        }
    }

    public async Task<int> BroadcastAsync(IEnumerable<string> mobiles, string message, CancellationToken ct)
    {
        int success = 0;
        foreach (var m in mobiles)
        {
            if (await SendMessageAsync(m, message, ct)) success++;
            await Task.Delay(800, ct); // polite delay — avoid WA ban
        }
        return success;
    }

    public void Dispose() => _http.Dispose();

    private sealed record QrPayload(string? QrBase64, bool Authenticated);
}
