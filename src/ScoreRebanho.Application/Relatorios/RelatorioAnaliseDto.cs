using System.Text.Json.Serialization;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Application.Relatorios;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "tipo")]
[JsonDerivedType(typeof(ResumoPecuaristaDto), "pecuarista")]
[JsonDerivedType(typeof(RelatorioCooperativaDto), "cooperativa")]
public abstract record RelatorioAnaliseDto;

public sealed record ResumoPecuaristaDto(DateTimeOffset CalculadaEmUtc, string VersaoPremissas,
    int Score, ClassificacaoRisco Classificacao, string ExplicacaoResumida,
    IReadOnlyList<AlertaAnaliseDto> Alertas, CapacidadeResumoDto Capacidade,
    decimal PesoVendaPorAnimalKg, FinanceiroResumoDto Financeiro,
    decimal LucroCenarioAdversoReais, IReadOnlyList<string> Avisos) : RelatorioAnaliseDto;

public sealed record CapacidadeResumoDto(decimal OcupacaoDecimal, decimal CapacidadeUa, decimal QuantidadeMaximaAnimais);

public sealed record FinanceiroResumoDto(decimal CustoTotalReais, decimal CapitalInicialReais, decimal ReceitaReais,
    decimal LucroReais, decimal? MargemDecimal, decimal? PrecoEquilibrioPorKgReais, string ExplicacaoPrecoEquilibrio);

public sealed record RelatorioCooperativaDto(AnaliseCompletaDto Analise) : RelatorioAnaliseDto;



public sealed record RelatorioRebanhoDto(Guid Id, string Nome, string Propriedade,
    Guid UltimoScoreId, RelatorioAnaliseDto Relatorio);

public sealed record RelatorioHistoricoDto(Guid Id, Guid? RebanhoOrigemId, string? NomeRebanho,
    string? Propriedade, RelatorioAnaliseDto Relatorio);
