using Microsoft.Extensions.Caching.Memory;
using ScoreRebanho.Application.Clima;

namespace ScoreRebanho.Infrastructure.Clima;

public sealed class CachePrevisoes(TimeProvider relogio) : IDisposable
{
    private static readonly TimeSpan Validade = TimeSpan.FromMinutes(30);
    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = 256 });

    public PrevisaoDto? Obter(decimal latitude, decimal longitude)
    {
        if (!cache.TryGetValue<PrevisaoDto>((latitude, longitude), out var previsao) || previsao is null)
            return null;
        var idade = relogio.GetUtcNow() - previsao.ConsultadaEmUtc;
        return idade >= TimeSpan.Zero && idade < Validade ? previsao : null;
    }

    public void Salvar(decimal latitude, decimal longitude, PrevisaoDto previsao) =>
        cache.Set((latitude, longitude), previsao, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = Validade, Size = 1 });

    public void Dispose() => cache.Dispose();
}
