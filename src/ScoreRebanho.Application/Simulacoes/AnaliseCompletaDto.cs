using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Application.Simulacoes;

// Resultado interno completo: a entrega ao navegador exige projeção e autorização por perfil.
public sealed record AnaliseCompletaDto(DateTimeOffset CalculadaEmUtc, EntradaSimulacaoDto Entradas,
    PremissasUtilizadasDto Premissas, LotacaoAnaliseDto Lotacao, ProducaoAnaliseDto Producao,
    CustosAnaliseDto Custos, FinanceiroAnaliseDto Financeiro, ScoreAnaliseDto Score,
    IReadOnlyList<AlertaAnaliseDto> Alertas, string ExplicacaoResumida, IReadOnlyList<string> Avisos)
{
    public IReadOnlyList<FormulaAnaliseDto> Formulas { get; init; } = Array.Empty<FormulaAnaliseDto>();
}

public sealed record FormulaAnaliseDto(string Indicador, string Expressao);

public sealed record LotacaoAnaliseDto(decimal UaLote, decimal MultiplicadorRotacao,
    decimal CapacidadeSemRotacaoUa, decimal CapacidadeUa, decimal OcupacaoDecimal,
    decimal QuantidadeMaximaAnimaisSemRotacao, decimal QuantidadeMaximaAnimais);

public sealed record ProducaoAnaliseDto(decimal FatorGmd, decimal GmdKgDia, decimal PesoVendaPorAnimalKg,
    decimal MortalidadeDecimal, decimal AnimaisVendidosEstimados, decimal PesoTotalVendidoKg, decimal ArrobasVivasVendidas);

public sealed record CustosAnaliseDto(decimal CustoTotalReais, decimal CapitalInicialReais,
    IReadOnlyList<ItemCustoDto> Itens, IReadOnlyList<ConsumoProdutoDto> Produtos);

public sealed record ItemCustoDto(string Descricao, decimal CustoOperacaoReais, decimal CapitalInicialReais);

public sealed record ConsumoProdutoDto(string Produto, decimal DosePorAnimal, string UnidadeDose,
    decimal Aplicacoes, decimal QuantidadeNecessaria, decimal QuantidadeEmbalagens, decimal CustoReais);

public sealed record FinanceiroAnaliseDto(decimal? PrecoEquilibrioPorKgReais, string? MotivoEquilibrioIndisponivel,
    string? MotivoMargemIndisponivel, CenarioFinanceiroDto Referencia, CenarioFinanceiroDto Adverso);

public sealed record CenarioFinanceiroDto(decimal PrecoArrobaVivaReais, decimal ReceitaReais, decimal LucroReais, decimal? MargemDecimal);

public sealed record ScoreAnaliseDto(int Total, ClassificacaoRisco Classificacao, IReadOnlyList<ComponenteScoreDto> Componentes);

public sealed record ComponenteScoreDto(string Nome, decimal Pontos, string Justificativa);

public sealed record AlertaAnaliseDto(string Codigo, string Mensagem, string Recomendacao);
