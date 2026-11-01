# Configuração das premissas

A seção Premissas de src/ScoreRebanho.Web/appsettings.json contém os valores aprovados da versão 1.0 para lotação, produção, custos, financeiro e score. A Infrastructure carrega essa seção na inicialização; os construtores e validações do Domain recusam parâmetros inválidos e versões divergentes. Chaves desconhecidas e valores sem conversão também impedem a inicialização. As fórmulas e as convenções da SPEC 13 permanecem no Domain.

Os provedores padrão do ASP.NET Core permitem sobrepor valores por appsettings.{Ambiente}.json, variáveis de ambiente ou argumentos de inicialização. Por exemplo, Premissas__Financeiro__PrecoVendaArrobaVivaReais=350.125 define o preço por arroba viva usando ponto decimal na configuração. Percentuais são frações: 0.10 significa 10%. Unidades aparecem nos nomes dos parâmetros. Totais de formação são derivados dos insumos e não são configurados.

Uma alteração aprovada nos parâmetros deve receber uma nova identificação em Versao nas cinco partes do conjunto. A igualdade das versões é validada; a identificação de uma nova versão é responsabilidade do operador. O arquivo base deve ser mantido completo; sobreposições podem ser parciais. Alterações só entram em vigor após reiniciar a aplicação, evitando trocar as premissas durante uma operação.

O mesmo conjunto imutável atende conferência de ocupação e cálculo definitivo, para ambos os perfis. Novos cálculos preservam os valores e a versão no snapshot. Consultas históricas continuam lendo o snapshot original, sem recalcular. Não há endpoint público nem tela administrativa para alterar parâmetros.

Testes MSTest verificam o arquivo de configuração distribuído contra todos os valores aprovados, sobreposição decimal, efeitos no cálculo/snapshot, preservação do resultado anterior, seção ausente, versões divergentes, parâmetros inválidos, estouro, texto inválido e chaves desconhecidas.
