# Estado da implementação

## Primeira etapa

- Solução .NET 10 e quatro camadas com referências direcionadas.
- Domain independente de bibliotecas de infraestrutura.
- Lotação: UA do lote, capacidade de verão/inverno, rotação com benefício a partir de três piquetes e teto de 1,6.
- Capacidade em animais arredondada para baixo; demais valores mantêm precisão decimal.
- Premissas de lotação imutáveis, identificadas como versão 1.0 e substituíveis por configuração futura.
- Testes MSTest para limites, validação, determinismo e estouro.
- Cadastro, login e logout foram adicionados com Identity e SQLite. Consulte autenticacao.md para detalhes e limitações da verificação. O CRUD de rebanhos permanece pendente.

## Convenções técnicas

- Ocupação interna usa fração decimal: 1 significa 100%.
- Quantidades máximas calculadas são decimais inteiros para não introduzir limite artificial de Int32.
- Rotação exige número positivo de piquetes; um ou dois piquetes são válidos, mas sem benefício de capacidade.
- O Domain protege invariantes com exceções. A Application deverá validar entradas e traduzir falhas esperadas, inclusive estouro, para FluentResults.
- A referência Web → Infrastructure destina-se exclusivamente à composição de dependências.

## Verificação nesta etapa

O SDK 10.0.402 está instalado. O build e o restore foram tentados, mas o NuGet falhou ao acessar a configuração do perfil do Windows. A tentativa com configuração local explícita também falhou no mesmo acesso.
O comando dotnet test --no-restore terminou sem saída de testes; isso não comprova execução de testes. Compilação e testes permanecem sem validação.

## Próximas entregas

1. Completar cálculo de produção, custos, indicadores financeiros e score com as convenções da SPEC 13.
2. Application com FluentResults, DTOs e operações de CRUD; contratos simples de usuário autenticado e persistência.
3. EF Core/SQLite, Identity e migrations; gravação atômica, concorrência otimista e preservação de históricos.
4. MVC com AutoMapper, formulários compartilhados e saídas autorizadas por perfil.
5. Testes de autorização, transações, concorrência e integração Web em banco isolado.

O documento docs/ideias-prototipo-v6.md citado pela SPEC não está disponível. A ordem detalhada do relatório depende dessa referência.


## Incremento 1 do CRUD — identificação do rebanho

- Adicionado IdentificacaoRebanho ao Domain, compartilhado por ambos os perfis.
- Nome e propriedade obrigatórios, com limites de 100 e 150 caracteres; identificações repetidas são permitidas.
- Convenção de normalização: remover apenas espaços das extremidades antes de validar o tamanho. Não alterar espaços internos ou capitalização. Coberta por testes.
- Tipo imutável, sem dependência de banco ou apresentação.
- Build do Domain executado com sucesso, sem avisos ou erros.
- Testes MSTest adicionados, mas a execução foi impedida na restauração com NU1301 por bloqueio de rede ao NuGet.
- Próximo incremento: completar os dados de entrada da simulação e suas validações; em seguida integrar cálculo e persistência antes de disponibilizar operações de salvar na interface.
- As telas de listar, cadastrar, editar e excluir rebanhos permanecem pendentes. Não foram modificadas fórmulas ou permissões.

## Incremento 2 — entradas completas da simulação

DadosSimulacao reúne lotação, posse, arrendamento, pasto pronto, preço de compra, dias, raça e manejo sanitário em tipos imutáveis. A validação de lotação existente é reutilizada.

Convenção explícita: terreno próprio não tem despesa de arrendamento; um valor residual não negativo é normalizado para zero. Valores negativos são recusados. Arrendado exige valor informado, permitindo zero. Preço de compra e dias devem ser positivos, conforme SPEC 13.

Verificação: restauração autorizada concluída; dotnet test no projeto Domain.Tests passou com 48 testes. A compilação geral encontrou arquivos bloqueados pela aplicação Web aberta; a aplicação não foi encerrada.

## Incremento 3 — produção

Adicionados ganho médio diário, fator de superlotação, peso de venda, mortalidade, animais vendidos, peso total e arrobas vivas, com premissas imutáveis da versão 1.0. O cálculo reutiliza a lotação; os perfis não interferem no motor. Mortalidade reduz somente vendas e preserva as entradas originais.

Verificação: 56 testes do Domain aprovados, incluindo limites de ocupação, piso do fator de GMD, teto de mortalidade, bônus de manejo/raça, determinismo e precisão fracionária. Custos, resultado financeiro e score ainda estão pendentes.

## Verificação completa após incrementos 2 e 3

A solução compilou usando --artifacts-path .local/verificacao, evitando arquivos em uso pela aplicação aberta. A suíte completa passou: 56 testes de Domain, 6 de Application e 12 de Web (74 no total). Cadastro, login, logout e migration de Identity agora foram validados pelos testes de integração.

Foi corrigida a expectativa do teste de logout: a rota MVC convencional retorna 404 para GET /Conta/Sair. O teste verifica que esse GET não encerra a sessão; o POST com CSRF continua encerrando. A paralelização dos projetos de teste foi declarada explicitamente. Não houve alteração de permissões ou comportamento de produção.

Próximo incremento na ordem aprovada: cálculo de custos, seguido de indicadores financeiros e score. Persistência de rebanhos, casos de uso, telas, relatórios por perfil e demais contas/administração continuam pendentes.

## Incremento 4 — custos e capital inicial

Implementada a composição de compra, manutenção, formação por insumo, rotação, arrendamento, suplementação e produtos sanitários. As premissas ficam centralizadas e são validadas na construção da calculadora. Coleções de saída são somente leitura.

Preservadas as convenções da SPEC 13: despesas usam animais iniciais; frascos sobre o volume total; aplicações incluem dia zero e último dia coincidente; verão aloca 1/5 da formação; capital inclui formação/rotação integrais e modificador; manutenção/arrendamento/suplementação ficam fora do capital. Rotação com menos de três piquetes continua gerando custo.

Verificação: 78 testes de domínio aprovados, incluindo limites de aplicações, embalagens, suplementação, premissas inválidas, precisão e estouro numérico. Próximo incremento: indicadores financeiros e cenário adverso.

## Incremento 5 — indicadores financeiros

Implementados receita, lucro/prejuízo, margem sobre custo e preço de equilíbrio por kg vivo. A conversão para arroba reutiliza as premissas de produção. O cenário adverso reduz somente o preço em 10%, preservando peso e custo.

Divisões sem denominador retornam indicador indisponível com explicação, sem inventar margem ou preço de equilíbrio. Valores negativos de entrada são recusados e prejuízo é preservado. Verificação: 88 testes de domínio aprovados. Próximo incremento: componentes e classificação do score.

## Incremento 6 — componentes e classificação do score

Implementados os quatro componentes com premissas centralizadas: lotação, sanidade, resultado financeiro e manejo/custos fixos. O total é limitado a 0–100, arredondado com empate para cima e somente então classificado nas faixas 80+, 60–79, 40–59 e 0–39. Componentes preservam precisão e justificativas; a coleção de saída não permite alterações.

Verificação completa: build da solução em .local/verificacao sem avisos ou erros; 134 testes aprovados (116 Domain, 6 Application e 12 Web). Nenhuma alteração de fórmulas aprovadas ou de permissões.

Ainda falta integrar as calculadoras no caso de uso de análise, gerar alertas/recomendações e registrar as premissas completas na resposta antes da persistência/histórico. Os valores derivados recebidos pelas calculadoras de domínio deverão ser produzidos pelo servidor; não são contratos HTTP. A camada Application ainda deverá traduzir entradas inválidas e estouro numérico em FluentResults. CRUD, telas de rebanhos e relatórios por perfil permanecem pendentes; a referência de interface docs/ideias-prototipo-v6.md continua ausente.
