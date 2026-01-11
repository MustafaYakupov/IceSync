using IceSync.Common.Options;
using IceSync.Infrastructure.UniversalLoader.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace IceSync.Infrastructure.UniversalLoader;

public class UniversalLoaderTokenProvider : IUniversalLoaderTokenProvider
{
    private const string CacheKey = "UL_JWT_TOKEN";
    private readonly IMemoryCache cache;
    private readonly HttpClient http;
    private readonly UniversalLoaderOptions options;

    private static bool IsExpired(string jwt) => DateTimeOffset.UtcNow >= GetExpiry(jwt);

    public UniversalLoaderTokenProvider(
        IMemoryCache cache,
        IHttpClientFactory factory,
        IOptions<UniversalLoaderOptions> options)
    {
        this.cache = cache;
        this.http = factory.CreateClient("UniversalLoaderAuth");
        this.options = options.Value;
    }


    public async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (this.cache.TryGetValue(CacheKey, out string? cached) &&
            !string.IsNullOrWhiteSpace(cached) &&
            !IsExpired(cached))
        {
            return cached!;
        }

        var payload = new
        {
            apiCompanyId = this.options.CompanyId,
            apiUserId = this.options.UserId,
            apiUserSecret = this.options.UserSecret
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");

        using var resp = await this.http.PostAsync("/authenticate", content, ct);
        resp.EnsureSuccessStatusCode();

        var raw = (await resp.Content.ReadAsStringAsync(ct)).Trim();
        var token = ExtractToken(raw);

        var exp = GetExpiry(token);
        var cacheFor = exp - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1);
        if (cacheFor < TimeSpan.FromSeconds(10)) cacheFor = TimeSpan.FromMinutes(1);

        this.cache.Set(CacheKey, token, cacheFor);

        return token;
    }

    private static string ExtractToken(string raw)
    {
        raw = raw.Trim();

        // token might be plain string OR JSON object
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("token", out var tokenProp))
                    return tokenProp.GetString()!.Trim();

                if (doc.RootElement.TryGetProperty("access_token", out var atProp))
                    return atProp.GetString()!.Trim();
            }
        }
        catch
        {
            // ignore
        }

        return raw.Trim('"');
    }

    private static DateTimeOffset GetExpiry(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        var exp = token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        return long.TryParse(exp, out var sec)
            ? DateTimeOffset.FromUnixTimeSeconds(sec)
            : DateTimeOffset.UtcNow.AddMinutes(5);
    }


}
