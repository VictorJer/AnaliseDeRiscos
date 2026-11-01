using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Domain.Custos;

public sealed class CalculadoraCustos
{
    private readonly PremissasCustos premissas;
    private readonly CalculadoraLotacao lotacao;

    public CalculadoraCustos(PremissasLotacao premissasLotacao, PremissasCustos premissas)
    {
        ArgumentNullException.ThrowIfNull(premissasLotacao);
        ArgumentNullException.ThrowIfNull(premissas);
        premissas.Validar();
        if (premissasLotacao.Versao != premissas.Versao)
            throw new ArgumentException("As premissas de lotação e custos devem ter a mesma versão.", nameof(premissas));
        this.premissas = premissas;
        lotacao = new(premissasLotacao);
    }

    public ResultadoCustos Calcular(DadosSimulacao dados)
    {
        ArgumentNullException.ThrowIfNull(dados);
        var ocupacao = lotacao.Calcular(dados.Lotacao).OcupacaoDecimal;
        var area = dados.Lotacao.AreaHectares;
        var animais = dados.Lotacao.QuantidadeAnimais;
        var dias = dados.DiasPastagem;
        var verao = dados.Lotacao.Pastagem == TipoPastagem.Verao;
        var itens = new List<ItemCusto>();
        var produtos = new List<ConsumoProduto>();

        var compra = animais * dados.Lotacao.PesoCompraKg * dados.PrecoCompraPorKgReais;
        itens.Add(new("Compra do gado", compra, compra));
        var manutencao = area * (verao ? premissas.ManutencaoVeraoReaisHaAno : premissas.ManutencaoInvernoReaisHaAno)
            * dias / premissas.DiasAno;
        itens.Add(new("Manutenção do pasto", manutencao, 0m));
        AdicionarFormacao(itens, dados);

        // Marcar rotação gera investimento mesmo quando não há benefício de capacidade.
        var rotacao = dados.Lotacao.RotacaoAtiva
            ? area * (premissas.RotacaoBaseReaisHa + premissas.RotacaoReaisHaPorPiquete * dados.Lotacao.NumeroPiquetes!.Value) : 0m;
        itens.Add(new("Rotação do pasto", rotacao * dias / (premissas.AnosAmortizacaoRotacao * (decimal)premissas.DiasAno), rotacao));
        itens.Add(new("Arrendamento", dados.ArrendamentoMensalReais * dias / premissas.DiasMesArrendamento, 0m));
        var suplementacao = animais * (decimal)dias * premissas.SuplementacaoReaisAnimalDia
            * Math.Max(0m, ocupacao - premissas.InicioSuplementacaoOcupacao);
        itens.Add(new("Suplementação", suplementacao, 0m));

        if (dados.Manejo.VacinaCarbunculo)
            produtos.Add(CalcularMedicamento("Vacina de carbúnculo", premissas.Carbunculo, animais, dias));
        if (dados.Manejo.ControleCarrapato)
            produtos.Add(CalcularMedicamento("Controle de carrapato", premissas.Carrapato, animais, dias));
        if (dados.Manejo.Vermifugo)
            produtos.Add(CalcularMedicamento("Vermífugo", premissas.Vermifugo, animais, dias));
        if (dados.Manejo.ModificadorOrganico)
        {
            var quantidade = animais * (decimal)dias * premissas.ModificadorKgAnimalDia;
            var sacos = decimal.Ceiling(quantidade / premissas.ModificadorKgPorSaco);
            produtos.Add(new("Modificador orgânico", premissas.ModificadorKgAnimalDia, "kg/animal/dia", dias,
                quantidade, sacos, sacos * premissas.ModificadorPrecoSacoReais));
        }
        foreach (var produto in produtos)
            itens.Add(new(produto.Produto, produto.CustoReais, produto.CustoReais));
        return new(premissas.Versao, itens, produtos);
    }

    private void AdicionarFormacao(List<ItemCusto> itens, DadosSimulacao dados)
    {
        if (dados.PastoPronto)
            return;
        var verao = dados.Lotacao.Pastagem == TipoPastagem.Verao;
        var formacao = verao ? premissas.FormacaoVerao : premissas.FormacaoInverno;
        var fracao = verao ? premissas.FracaoEconomicaFormacaoVerao : 1m;
        foreach (var componente in new (string Nome, decimal Preco)[]
        {
            ("Semente", formacao.SementeReaisPorHectare), ("Calcário", formacao.CalcarioReaisPorHectare),
            ("Adubo base", formacao.AduboBaseReaisPorHectare), ("Ureia", formacao.UreiaReaisPorHectare),
            ("Preparo e plantio", formacao.PreparoReaisPorHectare)
        })
        {
            var investimento = componente.Preco * dados.Lotacao.AreaHectares;
            itens.Add(new($"Formação do pasto: {componente.Nome}", investimento * fracao, investimento));
        }
    }

    private static ConsumoProduto CalcularMedicamento(string nome, ProdutoSanitario produto, int animais, int dias)
    {
        var aplicacoes = produto.IntervaloDias is { } intervalo ? 1m + dias / intervalo : 1m;
        var volume = animais * produto.DoseMlPorAnimal * aplicacoes;
        var frascos = decimal.Ceiling(volume / produto.VolumeFrascoMl);
        return new(nome, produto.DoseMlPorAnimal, "ml/animal/aplicação", aplicacoes, volume, frascos,
            frascos * produto.PrecoFrascoReais);
    }
}
