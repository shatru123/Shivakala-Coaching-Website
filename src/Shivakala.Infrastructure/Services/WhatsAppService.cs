using System.Net.Http.Json;
using System.Net;
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
        if (!TryConfigureClient(out var baseUris))
            return null;

        foreach (var baseUri in baseUris)
        {
            try
            {
                var resp = await _http.GetAsync(new Uri(baseUri, "/qr"), ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    _logger.LogWarning("WhatsApp sidecar rejected the QR request at {BaseUrl}. Check that WhatsApp:ApiKey matches WHATSAPP_API_KEY.", baseUri);
                    return null;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

                _configuredBaseUrl = baseUri.ToString().TrimEnd('/');
                var payload = await resp.Content.ReadFromJsonAsync<QrPayload>(ct);
                if (payload?.Authenticated == true) { _authenticated = true; return null; }
                if (string.IsNullOrWhiteSpace(payload?.QrBase64)) return null;
                return Convert.FromBase64String(payload.QrBase64.Split(',').Last());
            }
            catch (Exception ex)
            {
                _authenticated = false;
                _logger.LogWarning(ex, "WhatsApp sidecar unreachable at {BaseUrl} — QR fetch failed", baseUri);
            }
        }

        return null;
    }

    public async Task<bool> SendMessageAsync(string mobile, string message, CancellationToken ct)
    {
        if (!TryConfigureClient(out var baseUris))
            return false;

        foreach (var baseUri in baseUris)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync(new Uri(baseUri, "/send"), new { mobile, message }, ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    _logger.LogWarning("WhatsApp sidecar rejected the send request at {BaseUrl}. Check that WhatsApp:ApiKey matches WHATSAPP_API_KEY.", baseUri);
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

                _configuredBaseUrl = baseUri.ToString().TrimEnd('/');
                return true;
            }
            catch (Exception ex)
            {
                _authenticated = false;
                _logger.LogWarning(ex, "WhatsApp send failed to {Mobile} via {BaseUrl}", mobile, baseUri);
            }
        }

        return false;
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

    private bool TryConfigureClient(out IReadOnlyList<Uri> baseUris)
    {
        var baseUrl = NormalizeBaseUrl(_options.CurrentValue.BaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _authenticated = false;
            baseUris = Array.Empty<Uri>();
            _logger.LogInformation(
                "WhatsApp sidecar is disabled because '{Section}:{Key}' is not configured.",
                WhatsAppOptions.SectionName,
                nameof(WhatsAppOptions.BaseUrl));
            return false;
        }

        baseUris = GetCandidateBaseUris(baseUrl);
        _http.DefaultRequestHeaders.Remove("X-Api-Key");

        if (!string.IsNullOrWhiteSpace(_options.CurrentValue.ApiKey))
            _http.DefaultRequestHeaders.Add("X-Api-Key", _options.CurrentValue.ApiKey);

        _configuredBaseUrl = baseUrl;
        return true;
    }

    private static IReadOnlyList<Uri> GetCandidateBaseUris(string baseUrl)
    {
        var primary = new Uri(baseUrl, UriKind.Absolute);
        var uris = new List<Uri> { primary };

        if (string.Equals(primary.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            var fallbackBuilder = new UriBuilder(primary)
            {
                Scheme = Uri.UriSchemeHttp,
                Port = primary.IsDefaultPort ? 80 : primary.Port
            };

            uris.Add(fallbackBuilder.Uri);
        }

        return uris;
    }

    private static string? NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return null;

        return baseUrl.Trim().TrimEnd('/');
    }

    private sealed record QrPayload(string? QrBase64, bool Authenticated);
}
