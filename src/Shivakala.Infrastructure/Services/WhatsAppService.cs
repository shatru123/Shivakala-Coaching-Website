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
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<bool> CheckStatusAsync(CancellationToken ct = default)
    {
        if (!TryConfigureClient(out var baseUris))
        {
            _authenticated = false;
            return false;
        }

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateRequest(HttpMethod.Get, new Uri(baseUri, "/status"));
                using var resp = await _http.SendAsync(req, ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    _logger.LogWarning("WhatsApp sidecar rejected status request at {BaseUrl}. Check WhatsApp:ApiKey matches WHATSAPP_API_KEY.", baseUri);
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

                _configuredBaseUrl = baseUri.ToString().TrimEnd('/');
                var status = await resp.Content.ReadFromJsonAsync<StatusPayload>(ct);
                _authenticated = status?.Authenticated == true;
                return _authenticated;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WhatsApp sidecar status check failed at {BaseUrl}", baseUri);
            }
        }

        return _authenticated;
    }

    public async Task<byte[]?> GetQrCodeAsync(CancellationToken ct = default)
    {
        if (!TryConfigureClient(out var baseUris))
            return null;

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateRequest(HttpMethod.Get, new Uri(baseUri, "/qr"));
                using var resp = await _http.SendAsync(req, ct);
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
                if (payload?.Authenticated == true)
                {
                    _authenticated = true;
                    return null;
                }

                if (string.IsNullOrWhiteSpace(payload?.QrBase64))
                    return null;

                return Convert.FromBase64String(payload.QrBase64.Split(',').Last());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WhatsApp sidecar unreachable at {BaseUrl} — QR fetch failed", baseUri);
            }
        }

        return null;
    }

    public async Task<bool> SendMessageAsync(string mobile, string message, CancellationToken ct = default)
    {
        if (!TryConfigureClient(out var baseUris))
            return false;

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateRequest(HttpMethod.Post, new Uri(baseUri, "/send"));
                req.Content = JsonContent.Create(new { mobile, message });
                using var resp = await _http.SendAsync(req, ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    _logger.LogWarning("WhatsApp sidecar rejected the send request at {BaseUrl}. Check that WhatsApp:ApiKey matches WHATSAPP_API_KEY.", baseUri);
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

                _authenticated = true;
                _configuredBaseUrl = baseUri.ToString().TrimEnd('/');
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WhatsApp send failed to {Mobile} via {BaseUrl}", mobile, baseUri);
            }
        }

        return false;
    }

    public async Task<int> BroadcastAsync(IEnumerable<string> mobiles, string message, CancellationToken ct = default)
    {
        var mobileList = mobiles.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList();
        if (mobileList.Count == 0) return 0;

        if (TryConfigureClient(out var baseUris))
        {
            foreach (var baseUri in baseUris)
            {
                try
                {
                    using var req = CreateRequest(HttpMethod.Post, new Uri(baseUri, "/broadcast"));
                    req.Content = JsonContent.Create(new { mobiles = mobileList, message });
                    using var resp = await _http.SendAsync(req, ct);
                    if (resp.IsSuccessStatusCode)
                    {
                        var result = await resp.Content.ReadFromJsonAsync<BroadcastPayload>(ct);
                        _authenticated = true;
                        _configuredBaseUrl = baseUri.ToString().TrimEnd('/');
                        if (result != null) return result.Sent;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "WhatsApp bulk broadcast request failed via {BaseUrl}, falling back to per-recipient send", baseUri);
                }
            }
        }

        int success = 0;
        foreach (var m in mobileList)
        {
            if (await SendMessageAsync(m, message, ct)) success++;
            await Task.Delay(800, ct); // polite delay — avoid WA ban
        }
        return success;
    }

    public async Task<bool> DisconnectAsync(CancellationToken ct = default)
    {
        if (!TryConfigureClient(out var baseUris))
            return false;

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateRequest(HttpMethod.Post, new Uri(baseUri, "/disconnect"));
                using var resp = await _http.SendAsync(req, ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    _logger.LogWarning("WhatsApp sidecar rejected the disconnect request at {BaseUrl}. Check that WhatsApp:ApiKey matches WHATSAPP_API_KEY.", baseUri);
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

                _authenticated = false;
                _configuredBaseUrl = baseUri.ToString().TrimEnd('/');
                return true;
            }
            catch (Exception ex)
            {
                _authenticated = false;
                _logger.LogWarning(ex, "WhatsApp disconnect failed via {BaseUrl}", baseUri);
            }
        }

        return false;
    }

    public void Dispose() => _http.Dispose();

    private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri)
    {
        var req = new HttpRequestMessage(method, uri);
        var apiKey = _options.CurrentValue.ApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey) && !apiKey.StartsWith("__"))
        {
            req.Headers.Add("X-Api-Key", apiKey.Trim());
        }
        return req;
    }

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
        _configuredBaseUrl = baseUrl;
        return true;
    }

    private static IReadOnlyList<Uri> GetCandidateBaseUris(string baseUrl)
    {
        var primary = new Uri(baseUrl, UriKind.Absolute);

        // Always prefer HTTPS over HTTP to prevent HTTP -> HTTPS redirect header stripping
        if (string.Equals(primary.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            var httpsBuilder = new UriBuilder(primary)
            {
                Scheme = Uri.UriSchemeHttps,
                Port = primary.IsDefaultPort ? 443 : primary.Port
            };

            return new List<Uri> { httpsBuilder.Uri, primary };
        }

        var uris = new List<Uri> { primary };

        if (string.Equals(primary.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            var httpBuilder = new UriBuilder(primary)
            {
                Scheme = Uri.UriSchemeHttp,
                Port = primary.IsDefaultPort ? 80 : primary.Port
            };

            uris.Add(httpBuilder.Uri);
        }

        return uris;
    }

    private static string? NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return null;

        return baseUrl.Trim().TrimEnd('/');
    }

    private sealed record StatusPayload(bool Authenticated);
    private sealed record QrPayload(string? QrBase64, bool Authenticated);
    private sealed record BroadcastPayload(int Sent, int Failed, int Total);
}
