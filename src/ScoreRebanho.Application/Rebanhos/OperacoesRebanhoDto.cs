using ScoreRebanho.Application.Simulacoes;

namespace ScoreRebanho.Application.Rebanhos;

public sealed record CriarRebanhoDto(string Nome, string Propriedade, EntradaSimulacaoDto Entradas, decimal? Latitude = null, decimal? Longitude = null);
public sealed record AtualizarRebanhoDto(Guid Id, Guid Versao, string Nome, string Propriedade, EntradaSimulacaoDto Entradas, decimal? Latitude = null, decimal? Longitude = null);
public sealed record ConsultarRebanhoDto(Guid Id);
public sealed record ListarRebanhosDto(int Pagina = 1);
public sealed record ExcluirRebanhoDto(Guid Id, Guid Versao);
public sealed record DadosEdicaoRebanhoDto(Guid Id, Guid Versao, string Nome, string Propriedade, EntradaSimulacaoDto Entradas, decimal? Latitude = null, decimal? Longitude = null);
