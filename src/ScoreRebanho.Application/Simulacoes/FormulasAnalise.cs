namespace ScoreRebanho.Application.Simulacoes;

internal static class FormulasAnalise
{
    internal static IReadOnlyList<FormulaAnaliseDto> Todas { get; } = Array.AsReadOnly(new FormulaAnaliseDto[]
    {
        new("UA do lote", "animais iniciais × peso de compra kg ÷ quilogramas por UA"),
        new("Rotação", "sem piquetes suficientes: 1; caso contrário: mínimo(teto, 1 + incremento × (piquetes − 1))"),
        new("Capacidade", "área ha × capacidade da pastagem UA/ha × multiplicador de rotação"),
        new("Ocupação", "UA do lote ÷ capacidade UA; multiplicar por 100 para exibir em percentual"),
        new("Animais compatíveis", "piso(capacidade UA × quilogramas por UA ÷ peso de compra kg)"),
        new("GMD", "(GMD base + bônus de raça, carrapato, vermífugo e modificador selecionados) × fator de superlotação"),
        new("Fator de GMD", "máximo(piso do fator, 1 − penalidade × máximo(0, ocupação decimal − 1))"),
        new("Mortalidade", "mínimo(teto, mortalidade base + acréscimos sanitários + acréscimo × máximo(0, ocupação decimal − 1))"),
        new("Peso de venda", "peso de compra kg + GMD kg/dia × dias"),
        new("Produção vendida", "animais iniciais × (1 − mortalidade decimal) × peso de venda kg; arrobas = kg ÷ kg por arroba viva"),
        new("Compra", "animais iniciais × peso de compra kg × preço de compra por kg"),
        new("Manutenção", "área ha × manutenção da pastagem R$/ha/ano × dias ÷ dias por ano"),
        new("Formação", "investimento = área ha × soma dos insumos R$/ha; custo econômico = investimento × fração da pastagem"),
        new("Rotação: investimento e custo", "investimento = área ha × (valor base R$/ha + valor por piquete R$/ha × piquetes); custo = investimento × dias ÷ (anos de amortização × dias por ano)"),
        new("Arrendamento", "arrendamento mensal × dias ÷ dias por mês"),
        new("Suplementação", "animais iniciais × dias × valor R$/animal/dia × máximo(0, ocupação decimal − início da suplementação)"),
        new("Medicamentos", "aplicações = 1 + piso(dias/intervalo), ou 1 para dose única; volume = animais iniciais × dose ml × aplicações; frascos = teto(volume ÷ volume do frasco); custo = frascos × preço"),
        new("Modificador orgânico", "consumo kg = animais iniciais × dias × kg/animal/dia; sacos = teto(consumo ÷ kg por saco); custo = sacos × preço"),
        new("Custo e capital", "custo total = soma dos custos econômicos; capital = compra + investimentos integrais de formação/rotação + todos os produtos selecionados"),
        new("Financeiro", "receita = arrobas vendidas × preço; lucro = receita − custo total; margem = lucro ÷ custo total; equilíbrio R$/kg = custo total ÷ kg vendidos"),
        new("Cenário adverso", "preço adverso = preço de referência × (1 − queda); repetir os indicadores financeiros preservando produção e custos"),
        new("Score de lotação", "faixa ideal: máximo; abaixo: máximo − perda máxima × (1 − ocupação/mínimo ideal); acima: máximo × (ocupação sem pontos − ocupação)/(ocupação sem pontos − máximo ideal), limitado a zero"),
        new("Score de sanidade", "soma dos pontos de vacina, carrapato, vermífugo e modificador selecionados"),
        new("Score financeiro", "interpolação linear entre margem sem pontos e margem com pontuação máxima, limitada a esses extremos"),
        new("Score de manejo", "pontos de raça + prazo + posse própria + rotação com piquetes suficientes"),
        new("Score total", "soma dos componentes limitada ao máximo do score, arredondada com empate para cima antes de classificar pelas faixas salvas")
    });
}
