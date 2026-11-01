using FluentResults;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Application.Simulacoes;

internal static class PrepararSimulacao
{
    internal static Result<DadosSimulacao> Criar(EntradaSimulacaoDto entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        try
        {
            var lotacao = new DadosLotacao(entrada.AreaHectares, entrada.Pastagem, entrada.QuantidadeAnimais,
                entrada.PesoCompraKg, entrada.RotacaoAtiva, entrada.NumeroPiquetes);
            return Result.Ok(new DadosSimulacao(lotacao, entrada.Posse, entrada.ArrendamentoMensalReais,
                entrada.PastoPronto, entrada.PrecoCompraPorKgReais, entrada.DiasPastagem, entrada.RacaPuraCorte,
                new(entrada.VacinaCarbunculo, entrada.ControleCarrapato, entrada.Vermifugo, entrada.ModificadorOrganico)));
        }
        catch (ArgumentException ex) when (ex.ParamName is not null)
        {
            var falha = ex.ParamName switch
            {
                "areaHectares" => (nameof(entrada.AreaHectares), "Informe uma área em hectares maior que zero."),
                "pastagem" => (nameof(entrada.Pastagem), "Selecione uma pastagem válida."),
                "quantidadeAnimais" => (nameof(entrada.QuantidadeAnimais), "Informe uma quantidade de animais maior que zero."),
                "pesoCompraKg" => (nameof(entrada.PesoCompraKg), "Informe um peso de compra maior que zero."),
                "numeroPiquetes" => (nameof(entrada.NumeroPiquetes), "Informe piquetes positivos quando houver rotação; valores negativos não são permitidos."),
                "posse" => (nameof(entrada.Posse), "Selecione um tipo de posse válido."),
                "arrendamentoMensalReais" => (nameof(entrada.ArrendamentoMensalReais), "Informe um arrendamento não negativo quando o terreno for arrendado."),
                "precoCompraPorKgReais" => (nameof(entrada.PrecoCompraPorKgReais), "Informe um preço de compra por kg maior que zero."),
                "diasPastagem" => (nameof(entrada.DiasPastagem), "Informe uma quantidade de dias maior que zero."),
                _ => throw new InvalidOperationException("Falha de validação de entrada não reconhecida.", ex)
            };
            return Result.Fail<DadosSimulacao>(new Error(falha.Item2)
                .WithMetadata("Codigo", "ENTRADA_INVALIDA").WithMetadata("Campo", falha.Item1));
        }
    }
}
