using ScoreRebanho.Domain.Pastagens;

namespace ScoreRebanho.Domain.Producao;

public sealed record ResultadoProducao(ResultadoLotacao Lotacao,
    decimal FatorGmd, decimal GmdKgDia, decimal PesoVendaPorAnimalKg,
    decimal MortalidadeDecimal, decimal AnimaisVendidosEstimados,
    decimal PesoTotalVendidoKg, decimal ArrobasVivasVendidas);
