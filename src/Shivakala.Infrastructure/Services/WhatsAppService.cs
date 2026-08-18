using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Nodes;
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

    public string ActiveProvider => NormalizeProvider(_options.CurrentValue.Provider);

    public bool IsConfigured => ActiveProvider switch
    {
        "CloudApi" => !string.IsNullOrWhiteSpace(_options.CurrentValue.PhoneNumberId) &&
                      !string.IsNullOrWhiteSpace(_options.CurrentValue.AccessToken),
        "Sidecar" => !string.IsNullOrWhiteSpace(_options.CurrentValue.BaseUrl),
        "DirectLink" => true,
        _ => false
    };

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
        var provider = ActiveProvider;

        if (provider == "CloudApi")
        {
            var phoneId = _options.CurrentValue.PhoneNumberId?.Trim();
            var token = _options.CurrentValue.AccessToken?.Trim();

            if (string.IsNullOrWhiteSpace(phoneId) || string.IsNullOrWhiteSpace(token))
            {
                _authenticated = false;
                return false;
            }

            // Verify Meta Cloud API credentials
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"https://graph.facebook.com/v20.0/{phoneId}");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var resp = await _http.SendAsync(req, ct);
                _authenticated = resp.IsSuccessStatusCode;
                return _authenticated;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to verify Meta WhatsApp Cloud API status");
                _authenticated = false;
                return false;
            }
        }

        if (provider == "Sidecar")
        {
            return await CheckSidecarStatusAsync(ct);
        }

        _authenticated = true;
        return true;
    }

    public async Task<byte[]?> GetQrCodeAsync(CancellationToken ct = default)
    {
        if (ActiveProvider != "Sidecar")
            return null;

        if (!TryConfigureSidecarClient(out var baseUris))
            return null;

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateSidecarRequest(HttpMethod.Get, new Uri(baseUri, "/qr"));
                using var resp = await _http.SendAsync(req, ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    return null;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

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
        var provider = ActiveProvider;
        var normalizedMobile = NormalizeMobileNumber(mobile);
        if (string.IsNullOrWhiteSpace(normalizedMobile)) return false;

        if (provider == "CloudApi")
        {
            return await SendCloudApiMessageAsync(normalizedMobile, message, ct);
        }

        if (provider == "Sidecar")
        {
            return await SendSidecarMessageAsync(normalizedMobile, message, ct);
        }

        // DirectLink provider returns true (handled client-side)
        return true;
    }

    public async Task<int> BroadcastAsync(IEnumerable<string> mobiles, string message, CancellationToken ct = default)
    {
        var mobileList = mobiles
            .Select(NormalizeMobileNumber)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .ToList();

        if (mobileList.Count == 0) return 0;

        var provider = ActiveProvider;

        if (provider == "Sidecar")
        {
            return await BroadcastSidecarAsync(mobileList, message, ct);
        }

        int success = 0;
        foreach (var m in mobileList)
        {
            if (await SendMessageAsync(m!, message, ct)) success++;
            await Task.Delay(400, ct); // Rate limit for Meta Cloud API / Send
        }
        return success;
    }

    public async Task<bool> DisconnectAsync(CancellationToken ct = default)
    {
        if (ActiveProvider == "Sidecar")
        {
            return await DisconnectSidecarAsync(ct);
        }

        _authenticated = false;
        return true;
    }

    public string GenerateWhatsAppWebLink(string mobile, string message)
    {
        var normalized = NormalizeMobileNumber(mobile);
        var encodedMsg = Uri.EscapeDataString(message ?? string.Empty);
        return $"https://wa.me/{normalized}?text={encodedMsg}";
    }

    public void Dispose() => _http.Dispose();

    // ── Meta WhatsApp Cloud API Implementation ──────────────────────────────

    private async Task<bool> SendCloudApiMessageAsync(string mobile, string message, CancellationToken ct)
    {
        var phoneId = _options.CurrentValue.PhoneNumberId?.Trim();
        var token = _options.CurrentValue.AccessToken?.Trim();

        if (string.IsNullOrWhiteSpace(phoneId) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Meta WhatsApp Cloud API is missing PhoneNumberId or AccessToken.");
            return false;
        }

        try
        {
            var payload = new JsonObject
            {
                ["messaging_product"] = "whatsapp",
                ["recipient_type"] = "individual",
                ["to"] = mobile,
                ["type"] = "text",
                ["text"] = new JsonObject { ["body"] = message }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/v20.0/{phoneId}/messages");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = JsonContent.Create(payload);

            using var resp = await _http.SendAsync(req, ct);
            if (resp.IsSuccessStatusCode)
            {
                _authenticated = true;
                return true;
            }

            var errBody = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Meta WhatsApp Cloud API error ({Code}): {Response}", resp.StatusCode, errBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message via Meta WhatsApp Cloud API to {Mobile}", mobile);
            return false;
        }
    }

    // ── Sidecar Implementation ─────────────────────────────────────────────

    private async Task<bool> CheckSidecarStatusAsync(CancellationToken ct)
    {
        if (!TryConfigureSidecarClient(out var baseUris))
        {
            _authenticated = false;
            return false;
        }

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateSidecarRequest(HttpMethod.Get, new Uri(baseUri, "/status"));
                using var resp = await _http.SendAsync(req, ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

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

    private async Task<bool> SendSidecarMessageAsync(string mobile, string message, CancellationToken ct)
    {
        if (!TryConfigureSidecarClient(out var baseUris))
            return false;

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateSidecarRequest(HttpMethod.Post, new Uri(baseUri, "/send"));
                req.Content = JsonContent.Create(new { mobile, message });
                using var resp = await _http.SendAsync(req, ct);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authenticated = false;
                    return false;
                }

                if (!resp.IsSuccessStatusCode)
                    continue;

                _authenticated = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WhatsApp sidecar send failed to {Mobile} via {BaseUrl}", mobile, baseUri);
            }
        }

        return false;
    }

    private async Task<int> BroadcastSidecarAsync(List<string> mobileList, string message, CancellationToken ct)
    {
        if (TryConfigureSidecarClient(out var baseUris))
        {
            foreach (var baseUri in baseUris)
            {
                try
                {
                    using var req = CreateSidecarRequest(HttpMethod.Post, new Uri(baseUri, "/broadcast"));
                    req.Content = JsonContent.Create(new { mobiles = mobileList, message });
                    using var resp = await _http.SendAsync(req, ct);
                    if (resp.IsSuccessStatusCode)
                    {
                        var result = await resp.Content.ReadFromJsonAsync<BroadcastPayload>(ct);
                        _authenticated = true;
                        if (result != null) return result.Sent;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "WhatsApp sidecar bulk broadcast failed via {BaseUrl}", baseUri);
                }
            }
        }

        int success = 0;
        foreach (var m in mobileList)
        {
            if (await SendSidecarMessageAsync(m, message, ct)) success++;
            await Task.Delay(800, ct);
        }
        return success;
    }

    private async Task<bool> DisconnectSidecarAsync(CancellationToken ct)
    {
        if (!TryConfigureSidecarClient(out var baseUris))
            return false;

        foreach (var baseUri in baseUris)
        {
            try
            {
                using var req = CreateSidecarRequest(HttpMethod.Post, new Uri(baseUri, "/disconnect"));
                using var resp = await _http.SendAsync(req, ct);
                if (resp.IsSuccessStatusCode)
                {
                    _authenticated = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WhatsApp disconnect failed via {BaseUrl}", baseUri);
            }
        }

        return false;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static string NormalizeProvider(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return "CloudApi";
        if (provider.Equals("Sidecar", StringComparison.OrdinalIgnoreCase)) return "Sidecar";
        if (provider.Equals("DirectLink", StringComparison.OrdinalIgnoreCase)) return "DirectLink";
        return "CloudApi";
    }

    private static string NormalizeMobileNumber(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return string.Empty;
        var digits = new string(mobile.Where(char.IsDigit).ToArray());
        if (digits.Length == 10) return "91" + digits;
        if (digits.Length == 12 && digits.StartsWith("91")) return digits;
        return digits;
    }

    private HttpRequestMessage CreateSidecarRequest(HttpMethod method, Uri uri)
    {
        var req = new HttpRequestMessage(method, uri);
        var apiKey = _options.CurrentValue.ApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey) && !apiKey.StartsWith("__"))
        {
            req.Headers.Add("X-Api-Key", apiKey.Trim());
        }
        return req;
    }

    private bool TryConfigureSidecarClient(out IReadOnlyList<Uri> baseUris)
    {
        var baseUrl = _options.CurrentValue.BaseUrl?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _authenticated = false;
            baseUris = Array.Empty<Uri>();
            return false;
        }

        baseUris = GetCandidateBaseUris(baseUrl);
        return true;
    }

    private static IReadOnlyList<Uri> GetCandidateBaseUris(string baseUrl)
    {
        var primary = new Uri(baseUrl, UriKind.Absolute);
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

    private sealed record StatusPayload(bool Authenticated);
    private sealed record QrPayload(string? QrBase64, bool Authenticated);
    private sealed record BroadcastPayload(int Sent, int Failed, int Total);
}
