using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Configuration;

namespace Shivakala.Infrastructure.Services;

public sealed class WhatsAppService : IWhatsAppService, IDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly IOptionsMonitor<WhatsAppOptions> _options;
    private bool _authenticated;
    private string? _configuredBaseUrl;

    public bool IsAuthenticated => _authenticated;

    public WhatsAppService(
        ILogger<WhatsAppService> logger,
        IOptionsMonitor<WhatsAppOptions> options)
    {
        _logger = logger;
        _options = options;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    public async Task<byte[]?> GetQrCodeAsync(CancellationToken ct)
    {
        if (!TryConfigureClient())
            return null;

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
            _authenticated = false;
            _logger.LogWarning(ex, "WhatsApp sidecar unreachable at {BaseUrl} — QR fetch failed", _configuredBaseUrl);
            return null;
        }
    }

    public async Task<bool> SendMessageAsync(string mobile, string message, CancellationToken ct)
    {
        if (!TryConfigureClient())
            return false;

        try
        {
            var resp = await _http.PostAsJsonAsync("/send", new { mobile, message }, ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _authenticated = false;
            _logger.LogWarning(ex, "WhatsApp send failed to {Mobile} via {BaseUrl}", mobile, _configuredBaseUrl);
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

    private bool TryConfigureClient()
    {
        var baseUrl = NormalizeBaseUrl(_options.CurrentValue.BaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _authenticated = false;
            _logger.LogInformation(
                "WhatsApp sidecar is disabled because '{Section}:{Key}' is not configured.",
                WhatsAppOptions.SectionName,
                nameof(WhatsAppOptions.BaseUrl));
            return false;
        }

        if (string.Equals(_configuredBaseUrl, baseUrl, StringComparison.OrdinalIgnoreCase))
            return true;

        _http.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        _http.DefaultRequestHeaders.Remove("X-Api-Key");

        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.ApiKey))
            _http.DefaultRequestHeaders.Add("X-Api-Key", _options.CurrentValue.ApiKey);

        _configuredBaseUrl = baseUrl;
        return true;
    }

    private static string? NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return null;

        return baseUrl.Trim().TrimEnd('/');
    }

    private sealed record QrPayload(string? QrBase64, bool Authenticated);
}
