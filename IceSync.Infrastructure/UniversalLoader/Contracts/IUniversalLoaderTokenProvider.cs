namespace IceSync.Infrastructure.UniversalLoader.Contracts;

public interface IUniversalLoaderTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct);
}
