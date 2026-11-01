using ScoreRebanho.Web.ViewModels.Rebanhos;

namespace ScoreRebanho.Web.ViewModels.Relatorios;

public abstract class CabecalhoRelatorioViewModel
{
    public Guid? Id { get; set; }
    public Guid UltimoScoreId { get; set; }
    public string Nome { get; set; } = "";
    public string Propriedade { get; set; } = "";
    public bool Historico { get; set; }
}

public sealed class RelatorioPecuaristaViewModel : CabecalhoRelatorioViewModel
{
    public ResumoPecuaristaViewModel Resumo { get; set; } = null!;
}

public sealed class RelatorioCooperativaViewModel : CabecalhoRelatorioViewModel
{
    public AnaliseCompletaViewModel Analise { get; set; } = null!;
}

public sealed record ResumoPecuaristaViewModel(DateTimeOffset CalculadaEmUtc, string VersaoPremissas,
    int Score, string Classificacao, string ExplicacaoResumida, List<AlertaViewModel> Alertas,
    CapacidadeResumoViewModel Capacidade, decimal PesoVendaPorAnimalKg, FinanceiroResumoViewModel Financeiro,
    decimal LucroCenarioAdversoReais, List<string> Avisos);

public sealed record AlertaViewModel(string Codigo, string Mensagem, string Recomendacao);
public sealed record CapacidadeResumoViewModel(decimal OcupacaoDecimal, decimal CapacidadeUa, decimal QuantidadeMaximaAnimais);
public sealed record FinanceiroResumoViewModel(decimal CustoTotalReais, decimal CapitalInicialReais,
    decimal ReceitaReais, decimal LucroReais, decimal? MargemDecimal, decimal? PrecoEquilibrioPorKgReais,
    string ExplicacaoPrecoEquilibrio);

public sealed record AnaliseCompletaViewModel(DateTimeOffset CalculadaEmUtc, SimulacaoViewModel Entradas,
    List<ParametroViewModel> Premissas, LotacaoViewModel Lotacao, ProducaoViewModel Producao,
    CustosViewModel Custos, FinanceiroViewModel Financeiro, ScoreViewModel Score,
    List<AlertaViewModel> Alertas, string ExplicacaoResumida, List<string> Avisos,
    List<FormulaViewModel> Formulas);

public sealed record LotacaoViewModel(decimal UaLote, decimal MultiplicadorRotacao, decimal CapacidadeSemRotacaoUa,
    decimal CapacidadeUa, decimal OcupacaoDecimal, decimal QuantidadeMaximaAnimaisSemRotacao, decimal QuantidadeMaximaAnimais);
public sealed record ProducaoViewModel(decimal FatorGmd, decimal GmdKgDia, decimal PesoVendaPorAnimalKg,
    decimal MortalidadeDecimal, decimal AnimaisVendidosEstimados, decimal PesoTotalVendidoKg, decimal ArrobasVivasVendidas);
public sealed record CustosViewModel(decimal CustoTotalReais, decimal CapitalInicialReais,
    List<ItemCustoViewModel> Itens, List<ProdutoViewModel> Produtos);
public sealed record ItemCustoViewModel(string Descricao, decimal CustoOperacaoReais, decimal CapitalInicialReais);
public sealed record ProdutoViewModel(string Produto, decimal DosePorAnimal, string UnidadeDose,
    decimal Aplicacoes, decimal QuantidadeNecessaria, decimal QuantidadeEmbalagens, decimal CustoReais);
public sealed record FinanceiroViewModel(decimal? PrecoEquilibrioPorKgReais, string? MotivoEquilibrioIndisponivel,
    string? MotivoMargemIndisponivel, CenarioFinanceiroViewModel Referencia, CenarioFinanceiroViewModel Adverso);
public sealed record CenarioFinanceiroViewModel(decimal PrecoArrobaVivaReais, decimal ReceitaReais, decimal LucroReais, decimal? MargemDecimal);
public sealed record ScoreViewModel(int Total, string Classificacao, List<ComponenteScoreViewModel> Componentes);
public sealed record ComponenteScoreViewModel(string Nome, decimal Pontos, string Justificativa);
public sealed record ParametroViewModel(string Nome, string Valor);
public sealed record FormulaViewModel(string Indicador, string Expressao);
