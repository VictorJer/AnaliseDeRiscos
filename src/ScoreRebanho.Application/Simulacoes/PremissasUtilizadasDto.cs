using ScoreRebanho.Domain.Custos;
using ScoreRebanho.Domain.Financeiro;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Producao;
using ScoreRebanho.Domain.Scores;

namespace ScoreRebanho.Application.Simulacoes;

public sealed record PremissasUtilizadasDto(string Versao, PremissasLotacao Lotacao,
    PremissasProducao Producao, PremissasCustos Custos, PremissasFinanceiras Financeiro,
    PremissasScore Score, decimal ScoreMaximo, int LimiteRiscoBaixo, int LimiteRiscoModerado,
    int LimiteRiscoAlto, string ArredondamentoScore, string RegraAplicacoes, string RegraEmbalagens);
