namespace ScoreRebanho.Domain.Pastagens;

public sealed record ResultadoLotacao(
    string VersaoPremissas, decimal UaLote, decimal MultiplicadorRotacao,
    decimal CapacidadeSemRotacaoUa, decimal CapacidadeUa, decimal OcupacaoDecimal,
    decimal QuantidadeMaximaAnimaisSemRotacao, decimal QuantidadeMaximaAnimais);
