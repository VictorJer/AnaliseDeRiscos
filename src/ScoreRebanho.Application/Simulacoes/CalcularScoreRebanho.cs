using FluentResults;
using ScoreRebanho.Domain.Analises;
using ScoreRebanho.Domain.Custos;
using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Producao;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Application.Simulacoes;

public sealed class CalcularScoreRebanho
{
    private readonly PremissasAnalise premissas;
    private readonly TimeProvider tempo;
    private readonly CalculadoraProducao producao;
    private readonly CalculadoraCustos custos;
    private readonly CalculadoraFinanceira financeiro;
    private readonly CalculadoraScore score;
    private readonly AvaliadorRiscos riscos;

    public CalcularScoreRebanho(TimeProvider tempo) : this(PremissasAnalise.Versao1, tempo) { }

    public CalcularScoreRebanho(PremissasAnalise premissas, TimeProvider tempo)
    {
        ArgumentNullException.ThrowIfNull(premissas);
        ArgumentNullException.ThrowIfNull(tempo);
        this.premissas = premissas;
        this.tempo = tempo;
        producao = new(premissas.Lotacao, premissas.Producao);
        custos = new(premissas.Lotacao, premissas.Custos);
        financeiro = new(premissas.Producao, premissas.Financeiro);
        score = new(premissas.Lotacao, premissas.Score);
        riscos = new(premissas);
    }

    public Result<AnaliseCompletaDto> Executar(EntradaSimulacaoDto entrada)
    {
        var preparacao = PrepararSimulacao.Criar(entrada);
        if (preparacao.IsFailed)
            return Result.Fail<AnaliseCompletaDto>(preparacao.Errors);
        var dados = preparacao.Value;
        var calculadaEmUtc = tempo.GetUtcNow().ToUniversalTime();
        try
        {
            var producaoCalculada = producao.Calcular(dados);
            var custosCalculados = custos.Calcular(dados);
            var financeiroCalculado = financeiro.Calcular(custosCalculados.CustoTotalReais, producaoCalculada.PesoTotalVendidoKg);
            if (financeiroCalculado.Referencia.MargemDecimal is not { } margem)
                return Result.Fail<AnaliseCompletaDto>(new Error("Não foi possível calcular a margem para gerar o score. Revise os valores informados.")
                    .WithMetadata("Codigo", "MARGEM_INDISPONIVEL"));
            var scoreCalculado = score.Calcular(dados, producaoCalculada.Lotacao.OcupacaoDecimal, margem);
            var alertas = riscos.Avaliar(dados, producaoCalculada.Lotacao, financeiroCalculado);
            var resultado = ProjecaoAnalise.Completa(entrada with { ArrendamentoMensalReais = dados.ArrendamentoMensalReais },
                premissas, producaoCalculada, custosCalculados, financeiroCalculado, scoreCalculado, alertas,
                calculadaEmUtc);
            return Result.Ok(resultado);
        }
        catch (OverflowException)
        {
            return FalhaNumerica();
        }
        catch (DivideByZeroException)
        {
            return FalhaNumerica();
        }
    }

    private static Result<AnaliseCompletaDto> FalhaNumerica() =>
        Result.Fail<AnaliseCompletaDto>(new Error("Os valores informados não permitem um cálculo válido. Revise a escala dos dados.")
            .WithMetadata("Codigo", "LIMITE_NUMERICO"));
}
