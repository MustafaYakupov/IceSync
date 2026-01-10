using IceSync.Common.Options;
using IceSync.Infrastructure.UniversalLoader.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;

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
        if (this.cache.TryGetValue(CacheKey, out string? token) &&
            !string.IsNullOrWhiteSpace(token) &&
            !IsExpired(token))
        {
            return token!;
        }

        // TODO: adjust endpoint/path to match real API
        var payload = new
        {
            apiCompanyId = this.options.CompanyId,
            apiUserId = this.options.UserId,
            apiUserSecret = this.options.UserSecret
        };

        using var resp = await http.PostAsJsonAsync("/auth/token", payload, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Empty token response.");

        token = json.Token;

        var exp = GetExpiry(token);
        var cacheFor = exp - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1);
        if (cacheFor < TimeSpan.FromSeconds(10)) cacheFor = TimeSpan.FromMinutes(1);

        this.cache.Set(CacheKey, token, cacheFor);
        return token!;
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

    private sealed class TokenResponse
    {
        public string Token { get; set; } = "";
    }
}
