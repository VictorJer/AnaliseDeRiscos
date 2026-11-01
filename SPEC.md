# SPEC.md — Score do Rebanho

## 1. Objetivo

Desenvolver uma aplicação web em C# para apoiar pecuaristas e cooperativas na análise de operações de gado de corte em regime extensivo na região serrana. Os dois perfis percorrem as mesmas etapas e usam as mesmas regras de cálculo; a diferença é o nível de informação disponibilizado no relatório final, conforme a seção 17. Antes de comprar o lote ou solicitar crédito, o usuário informa dados do terreno, do rebanho e do manejo. O sistema estima capacidade de pastagem, custos, resultado financeiro e um score de risco de 0 a 100, explicando os fatores que influenciaram o resultado.

O score é uma estimativa para apoiar o planejamento. Não garante lucro, segurança da operação ou aprovação de crédito.

## 2. Público e princípios de produto

- Usuário principal: pecuarista, que precisa de um fluxo curto, linguagem simples e controles fáceis de usar.
- Usuário secundário: analista de crédito agro, que precisa entender as premissas e os números apresentados.
- Interface responsiva, adequada a celular e computador, com textos diretos, campos agrupados e mensagens de validação claras.
- Não apresentar resultado até que os dados obrigatórios sejam válidos e o usuário acione **Gerar score**.
- Explicar cada alerta e cada componente da pontuação; não exibir somente um número.
- Marcar resultados como estimativas e exibir as premissas usadas.

## 3. Escopo do MVP

### Incluído

1. Formulário guiado dividido em terreno, lote e saúde/manejo.
2. Validação de entradas e cálculo de resultados no servidor.
3. Relatório com score, classificação, fatores de risco, recomendações, previsão de peso, custos, receita, lucro/margem, ponto de equilíbrio, capital inicial estimado, capacidade do pasto e itens de saúde.
4. Ações **Editar dados** (preservando o preenchimento) e **Novo score** (limpando os dados).
5. Valores de referência centralizados e configuráveis, sem estarem espalhados pela interface ou pelos controllers.
6. Testes automatizados para regras e cálculos de domínio, adicionados junto à implementação.
7. Autenticação e gestão de contas com ASP.NET Core Identity, persistidas com Entity Framework Core.

### Fora do MVP inicial

- Cadastro de produtores como entidade de negócio e persistência/histórico de simulações.
- Integração automática com cotações ou fontes externas.
- Envio do resultado ao banco ou decisão de crédito.
- Juros e fluxo detalhado do financiamento, salvo como evolução posterior.
- Simulação probabilística de clima, preço ou mortalidade.
- Aplicativo móvel nativo.

A arquitetura deve permitir adicionar persistência e atualização administrativa de premissas depois, sem mover regras de negócio para a camada web.

Autenticação de usuários é feita com ASP.NET Core Identity. O uso de Identity não implica, por si só, papéis ou permissões de crédito; regras de autorização específicas devem ser definidas quando as funcionalidades correspondentes entrarem no escopo.

## 4. Arquitetura e projetos

Usar ASP.NET Core MVC com Razor Views e uma solução .NET organizada em quatro projetos:

```text
ScoreRebanho.sln
src/
  ScoreRebanho.Domain/
  ScoreRebanho.Application/
  ScoreRebanho.Infrastructure/
  ScoreRebanho.Web/
tests/
  ScoreRebanho.Domain.Tests/
  ScoreRebanho.Application.Tests/
```

Dependências permitidas:

```text
Domain          → nenhuma camada da aplicação
Application     → Domain
Infrastructure  → Application + Domain
Web             → Application
Web (composition root) configura implementações de Infrastructure
```

- **Domain**: entidades/objetos de valor, regras puras, cálculos e políticas de score. Sem dependência de ASP.NET Core, Entity Framework ou banco.
- **Application**: casos de uso, contratos de entrada/saída, orquestração e interfaces de serviços/repositórios quando necessárias. Caso de uso principal: `CalcularScoreRebanho`.
- **Infrastructure**: implementações técnicas, acesso a dados e carregamento de configurações. A persistência necessária ao ASP.NET Core Identity fica nesta camada com EF Core; não adicionar histórico de simulações ou outras tabelas de negócio até entrarem no escopo.
- **Web / Apresentação**: controllers MVC, ViewModels, Razor Views, validação de formulário, tratamento de erros e composição de dependências. Controllers não calculam score nem custos.

Preferir nomes e mensagens em português na interface. Código pode seguir convenção C# em português ou inglês, mas manter uma única convenção consistente.

## 5. Fluxo principal

1. Usuário abre a tela de nova simulação.
2. Preenche terreno, lote e manejo/saúde.
3. A aplicação valida os dados; entradas inválidas retornam à tela com os valores já preenchidos e erros próximos aos campos.
4. Ao submeter **Gerar score**, a camada Web mapeia o ViewModel para um comando da Aplicação.
5. A Aplicação chama o domínio para calcular os indicadores e score usando as premissas vigentes.
6. A tela de relatório apresenta o resultado e as premissas.
7. **Editar dados** volta ao formulário preenchido. **Novo score** inicia simulação limpa.

Não confiar em valores calculados enviados pelo navegador. Recalcular no servidor a cada submissão.

## 6. Dados de entrada

### Terreno

- Área em hectares (obrigatório, maior que zero).
- Tipo de posse: próprio ou arrendado.
- Valor do arrendamento mensal (obrigatório se arrendado; não negativo).
- Tipo de pastagem: verão ou inverno.
- Pasto pronto: sim/não.
- Rotação de pastagem: sim/não.
- Número de piquetes, obrigatório quando rotação estiver ativa; rotação só produz efeito com pelo menos 3 piquetes.

### Lote

- Quantidade de animais (inteiro positivo).
- Peso médio de compra em kg (positivo).
- Preço pago por kg vivo em reais (positivo ou zero, conforme regra de validação definida pelo produto; padrão recomendado: maior que zero).
- Dias na pastagem (inteiro positivo).
- Raça pura para corte: sim/não.

### Saúde e manejo

- Vacina de carbúnculo: sim/não.
- Controle de carrapato: sim/não.
- Vermífugo: sim/não.
- Modificador orgânico: sim/não.

Usar tipos apropriados (`decimal` para valores e pesos; `int` para quantidades/dias; enums para escolhas). Arredondar apenas para exibição, preservando precisão interna nos cálculos.

## 7. Premissas e regras iniciais do cálculo

As regras abaixo são a configuração inicial descrita no documento do MVP. Implementá-las de forma isolada e identificável, para facilitar validação e atualização. Valores não devem ficar embutidos em controllers ou views.

### Conversões e lotação

- 1 arroba de animal vivo = 30 kg (convenção adotada pelo grupo).
- 1 UA = 450 kg de peso vivo.
- UA do lote = animais × peso médio de compra ÷ 450.
- Capacidade base: verão = 1,0 UA/ha; inverno = 1,2 UA/ha.
- Com rotação e pelo menos 3 piquetes: multiplicador = `min(1,6; 1 + 0,15 × (piquetes - 1))`; caso contrário = 1,0.
- Capacidade total em UA = área × capacidade base × multiplicador de rotação.
- Ocupação percentual = UA do lote ÷ capacidade total em UA × 100.
- Até 100%: sem penalidade de superlotação.
- Acima de 100%: fator de GMD = `max(0,45; 1 - 0,5 × (ocupação decimal - 1))`.
- Acima de 100%: somar 4 pontos percentuais de mortalidade por cada 100 pontos percentuais acima da capacidade (implementar a regra explicitamente e aplicar o teto de mortalidade).
- Acima de 110%: custo adicional de suplementação de R$ 3,00 por animal por dia para cada 100 pontos percentuais de ocupação acima de 110%, conforme interpretação adotada no código do MVP; tornar fórmula visível e fácil de alterar.
- Abaixo de 50%: reduzir parcialmente a pontuação de lotação; ver regra de score.

### Ganho de peso e mortalidade

- GMD base extensivo = 0,55 kg/dia.
- Ajustes de GMD: raça pura +0,10; controle de carrapato +0,04; vermífugo +0,05; modificador orgânico +0,12 kg/dia.
- Aplicar o fator de superlotação ao GMD calculado.
- Peso de venda por animal = peso de compra + GMD ajustado × dias.
- Mortalidade base = 1,5%.
- Sem carbúnculo: +1,5 ponto percentual; sem controle de carrapato: +1,2 pp; sem vermífugo: +0,8 pp.
- Aplicar acréscimo de superlotação conforme a regra acima; limitar mortalidade total a 15%.
- Animais vendidos estimados = animais iniciais × (1 − mortalidade).
- Peso total vendido em kg = animais vendidos estimados × peso de venda por animal.
- Arrobas vendidas = peso total vendido em kg ÷ 30.

### Custos

- Compra do gado = animais × peso médio de compra × preço pago por kg vivo.
- Manutenção de pasto por ano: verão R$ 300/ha; inverno R$ 450/ha. Proporcionalizar pelos dias de uso (dias/365).
- Formação do pasto, quando não estiver pronto: custos por hectare da tabela abaixo. Verão contabiliza 1/5 do custo total; inverno contabiliza o custo total.
- Formação verão (R$/ha): semente 420; calcário 375; adubo base 640; ureia 350; preparo/plantio 600; total 2.385.
- Formação inverno (R$/ha): semente 425; calcário 250; adubo base 800; ureia 525; preparo/plantio 400; total 2.400.
- Rotação: investimento = área × (R$ 150 + R$ 60 × número de piquetes), amortizado em 10 anos. Definir e documentar a proporcionalização ao período analisado (dias/3650) para custo econômico; exibir separadamente investimento total e parcela atribuída ao período. Para capital inicial, usar o investimento total se essa for a convenção de crédito escolhida.
- Arrendamento = valor mensal × dias ÷ 30.
- Medicamentos: calcular volume necessário por animal e número de frascos inteiros, arredondando frascos para cima.
  - Carbúnculo: dose 2 ml/animal, 1 aplicação; frasco 100 ml a R$ 40.
  - Carrapato: dose 4 ml/animal, 1 aplicação a cada 90 dias; frasco 500 ml a R$ 100. Aplicações no período: incluir aplicação inicial e as que vencerem até o fim do período; regra deve ser explicitada e validada.
  - Vermífugo: dose 6 ml/animal, 1 aplicação a cada 150 dias; frasco 500 ml a R$ 85. Aplicações no período: explicitar a convenção de aplicação inicial e intervalos.
  - Modificador orgânico: 0,05 kg/animal/dia; saco de 25 kg a R$ 120; sacos arredondados para cima.
- Gasto total deve somar os itens de custo considerados na operação. Mostrar composição discriminada.
- Capital inicial estimado: compra do gado + formação do pasto conforme regra de capital + rotação conforme regra de capital + medicamentos. Apresentar os componentes e indicar claramente que manutenção, arrendamento e suplementação podem ocorrer durante a operação e se estão fora desse indicador.

### Resultado financeiro

- Preço de referência inicial de venda = R$ 340/@.
- Preço por kg vivo equivalente = preço/@ ÷ 30.
- Receita = arrobas vendidas × preço de referência.
- Lucro estimado = receita − gasto total.
- Margem = lucro ÷ gasto total; tratar gasto total zero sem divisão por zero.
- Preço de equilíbrio por kg vivo = gasto total ÷ kg vendidos; se não houver kg vendidos, retornar resultado indisponível com explicação.
- Apresentar cenário de queda de 10% no preço da arroba.

### Score (0–100)

Somar quatro componentes e limitar resultado final ao intervalo de 0 a 100:

1. **Lotação (30 pontos)**: 30 pontos entre 50% e 100% de ocupação; acima de 100%, decrescer linearmente até 0 em 180%; abaixo de 50%, perder até 12 pontos proporcionalmente ao afastamento de 50% (0% de ocupação equivale a 18 pontos).
2. **Sanidade (25 pontos)**: carbúnculo 8; carrapato 7; vermífugo 7; modificador orgânico 3. Conceder pontos quando o respectivo item estiver marcado como realizado/ativo.
3. **Resultado financeiro (30 pontos)**: margem de 20% ou mais = 30; margem de −10% ou menos = 0; interpolação linear entre os limites.
4. **Manejo e custos fixos (15 pontos)**: raça pura 6; prazo entre 120 e 365 dias inclusive 3 (fora desse intervalo 1); terreno próprio 2; rotação ativa com pelo menos 3 piquetes 4.

Classificação:

- 80–100: **Risco baixo**.
- 60–79: **Risco moderado**.
- 40–59: **Risco alto**.
- 0–39: **Risco muito alto**.

Gerar motivos e recomendações a partir das regras que dispararam os alertas (ex.: ocupação excede capacidade, ausência de manejo sanitário ou margem baixa). Não usar mensagens que prometam aprovação de crédito.

## 8. Relatório

Apresentar, no mínimo:

- Score total, faixa de risco e explicação resumida.
- Pontos por componente e fatores que contribuíram para cada pontuação.
- Alertas prioritários e recomendações acionáveis.
- Ocupação atual, capacidade em UA e estimativa de animais compatível com a área, com e sem rotação quando aplicável.
- GMD usado, mortalidade estimada, peso de venda por animal e total vendido.
- Custos discriminados, capital inicial estimado e premissas de capital.
- Receita, lucro, margem, preço de equilíbrio e cenário de preço −10%.
- Aviso de que são estimativas e os valores de referência devem ser confirmados na região.

Usar formatação monetária brasileira (R$), separador decimal local e unidades em todos os valores.

## 9. Requisitos de qualidade

- Validação no servidor para todas as entradas; validação no cliente pode complementar, nunca substituir.
- Proteção contra valores negativos, divisão por zero, estouro e resultados não finitos.
- Cálculo determinístico: mesmas entradas e mesmas premissas produzem o mesmo resultado.
- Arredondamento consistente apenas na apresentação; explicar critérios de arredondamento de frascos, sacos e moeda.
- Interface acessível por teclado, labels associados aos campos e contraste legível.
- Não incluir dados pessoais sensíveis sem necessidade. No escopo inicial, contas Identity são obrigatórias para simular; não há salvamento de simulações.
- Registrar as premissas e a versão do conjunto de parâmetros usado em cada cálculo, ao menos na resposta do caso de uso, para permitir rastreabilidade futura.

## 10. Critérios de aceite

- O usuário consegue preencher e submeter uma simulação válida do início ao relatório.
- Entradas inválidas não geram resultado e são apresentadas junto ao campo correspondente, preservando o preenchimento.
- É possível editar e recalcular sem redigitar e iniciar uma simulação limpa.
- Todos os cálculos ocorrem no servidor e os totais do relatório são coerentes com a composição exibida.
- Componentes do score totalizam no máximo 100 e a classificação respeita os limites definidos.
- Casos de ocupação abaixo de 50%, dentro de 50–100%, acima de 100% e acima de 180% têm comportamento definido e coberto por testes.
- Casos de mortalidade limitada a 15%, período sem aplicações repetidas e arredondamento de embalagens têm comportamento definido e coberto por testes.
- Layout é utilizável em tela estreita e não exige conhecimento técnico do usuário.

## 11. Pendências de produto a resolver durante a implementação

As regras originais têm alguns pontos que precisam de decisão explícita para evitar resultados contraditórios. Antes de fechar a primeira versão, documentar as convenções adotadas e mantê-las configuráveis quando razoável:

1. Se o preço de venda de R$ 340/@ é por arroba de animal vivo, garantir coerência com a conversão de 30 kg vivos por arroba em todos os cálculos e exemplos.
2. Definir se a mortalidade é calculada apenas para estimar animais vendidos ou se também reduz a compra/custos de medicamentos e suplementos.
3. Definir o significado operacional de “aplicações a cada 90/150 dias”, inclusive se há dose no dia inicial e quantas aplicações entram no período.
4. Definir como a suplementação acima de 110% cresce conforme o excedente de ocupação.
5. Distinguir custo de formação alocado ao período, investimento total e necessidade de capital inicial; não somar valores diferentes sem identificá-los.
6. Definir a fórmula exata para estimar a quantidade máxima de animais, inclusive peso de referência usado na conversão para UA.
7. Definir casas decimais e arredondamentos do score e do limite entre as faixas (o score final exibido deve ser inteiro; usar faixas 80+, 60–79, 40–59, 0–39).

Não ocultar divergências: refletir as decisões escolhidas em testes, no relatório e na versão das premissas.

## 12. Diretrizes de implementação

- Começar pela solução e projetos, depois implementar os tipos de domínio e cálculos puros, em seguida o caso de uso, e por fim o fluxo MVC e o relatório.
- Manter entrada/saída da Aplicação separada dos ViewModels da Web.
- Evitar controllers gordos, lógica de negócio em Razor/JavaScript e abstrações sem uso real.
- Não adicionar persistência de simulações, integrações externas ou abstrações sem necessidade para o escopo aprovado. A autenticação usa ASP.NET Core Identity, com stores EF Core na Infrastructure.
- Adicionar testes automatizados conforme cada regra é implementada, usando os casos numéricos do documento como referência após resolver as pendências acima.


## 13. Convenções aprovadas — versão 1.0

- Login obrigatório; SQLite persiste somente contas Identity. Cadastro, login, logout e alteração de senha; sem envio de e-mail nesta versão.
- R$ 340 por arroba viva de 30 kg. Preço de compra obrigatoriamente positivo.
- Mortalidade reduz apenas vendas; despesas usam animais iniciais. Quantidade vendida estimada pode ser fracionária.
- Aplicações: 1 + piso(dias/intervalo), incluindo dia zero e último dia coincidente. Frascos calculados sobre volume total; embalagens sempre arredondadas para cima.
- Suplementação = animais × dias × R$ 3 × máximo(0, ocupação decimal − 1,10).
- Mortalidade adicional = 0,04 × máximo(0, ocupação decimal − 1), com teto total 15%.
- Formação econômica: verão 1/5, inverno integral; capital considera investimento integral quando pasto não pronto.
- Rotação marcada gera custo mesmo abaixo de três piquetes; somente três ou mais geram benefícios. Amortização dias/3650; capital integral.
- Capital inclui compra, formação, rotação e todos os produtos selecionados, inclusive modificador. Exclui manutenção, arrendamento e suplementação.
- Capacidade em animais = piso(capacidade UA × 450 / peso de compra).
- Score limitado a 0–100 e arredondado com empate para cima antes da classificação. Exibição com duas casas, exceto GMD com três e score inteiro. Os componentes podem diferir visualmente do inteiro por arredondamento.
- Parâmetros inválidos impedem inicialização; estouro numérico de entradas retorna falha esperada sem relatório parcial.
- Cenário adverso altera somente preço/receita e seus resultados financeiros, preservando produção e custos.

## 14. Ampliação aprovada: administração de scores

O histórico de simulações passa a fazer parte do escopo. Cada cálculo válido é salvo com autor, data UTC, entradas, resultado e premissas utilizadas. Novos cálculos e recálculos geram registros distintos. Dados inválidos não são salvos. Resultados anteriores à implementação não podem ser recuperados.

Somente contas com papel Identity `Administrador` podem listar os scores de todos os usuários e abrir relatórios históricos. Consulta paginada em 20 registros, mais recentes primeiro, sem edição ou exclusão. Cadastro público nunca concede esse papel. A atribuição inicial ocorre por comando local explícito a uma conta já cadastrada. Esta seção substitui as exclusões anteriores de histórico e de papéis para esta funcionalidade.

## 15. Evolução da interface — protótipo v6 (P1)

O formulário oferece terreno, lote, manejo e conferência da ocupação no servidor antes de gerar o score. O resumo não cria registro no histórico e não exibe pontuação nem resultado financeiro. O cálculo definitivo continua validando e recalculando todas as entradas. Sem JavaScript, o formulário completo permite atualizar resumo e gerar relatório.

O relatório destaca score, componentes, indicadores principais e recomendações na ordem de atenção documentada em `docs/ideias-prototipo-v6.md`. Fórmulas, limites e premissas da versão 1.0 permanecem vigentes. Tema claro/escuro acompanha o sistema ou a preferência escolhida. Localização, propriedades, comparação/exclusão, clima, mercado e IA ainda dependem das decisões de produto desse documento.

Nota de rastreabilidade da referência visual: `docs/ideias-prototipo-v6.md` foi reconstruído a partir desta SPEC e das Views existentes, pois o protótipo original não foi localizado. O documento registra a organização atual e suas limitações; não declara fidelidade visual ao original nem altera fórmulas, permissões ou escopo. A inspeção em navegador tem situação própria em `docs/verificacao-interface.md`.

## 16. Home e CRUD de rebanhos

A home autenticada lista somente os rebanhos do usuário, em páginas de 20, ordenados pela última atualização. Cada cadastro tem nome (obrigatório, até 100 caracteres), propriedade (identificação textual obrigatória, até 150), entradas da simulação e último resultado. Nomes repetidos são permitidos; propriedade não é um cadastro separado nesta entrega.

Criar ou editar valida as entradas e recalcula no servidor com as premissas vigentes, salvando cadastro e novo score histórico atomicamente. Ler o relatório não recalcula. Atualizações concorrentes são recusadas para evitar sobrescrever dados de outra sessão. Os scores anteriores continuam imutáveis na administração.

A exclusão requer confirmação e POST protegido contra CSRF. Remove definitivamente o cadastro da home, mantendo os scores administrativos anteriores. Acesso, edição e exclusão sempre filtram o autor autenticado, inclusive para administradores nesta interface. Identificadores de outros usuários retornam não encontrado.

Simulações avulsas anteriores não são convertidas automaticamente em rebanhos, pois não têm nome/propriedade. O fluxo de nova simulação continua disponível. Comparação de versões, cadastro separado de propriedades e integrações externas permanecem fora desta entrega.
## 17. Ampliação: cooperativa e pecuarista

### 17.1. Objetivo e precedência

A aplicação atende dois perfis de negócio: **Pecuarista** e **Cooperativa**. Ambos executam os mesmos passos, informam os mesmos dados obrigatórios e utilizam as mesmas premissas, validações e fórmulas. A diferença funcional entre esses perfis é a abrangência das informações apresentadas ao final.

Esta seção atualiza o público definido na seção 2 e especializa o relatório da seção 8 por perfil. Complementa as seções 3, 5, 9, 10, 13, 14, 15 e 16. Permanecem vigentes os cálculos, o histórico, a administração e o CRUD de rebanhos já aprovados. Nenhum perfil recebe fórmulas, pontuação ou resultados financeiros diferentes para as mesmas entradas e premissas.

### 17.2. Perfis e autorização

- **Pecuarista**: recebe as informações essenciais para entender a viabilidade estimada da operação, os principais riscos e as ações recomendadas.
- **Cooperativa**: recebe todas as informações da análise, incluindo dados informados, indicadores, composição dos cálculos, premissas e justificativas técnicas e financeiras.
- **Administrador**: mantém as permissões da seção 14. É uma permissão administrativa independente dos perfis de negócio; uma cooperativa não é administradora automaticamente.

Cada conta tem um único perfil de negócio ativo. Cadastro público cria contas Pecuarista. A habilitação de Cooperativa exige atribuição explícita por operador autorizado, inicialmente por comando local, sem permitir que o usuário obtenha esse perfil alterando campos do formulário, da URL ou da requisição.

O perfil é obtido da identidade autenticada no servidor. Alterar o perfil não modifica entradas, resultados ou premissas históricos; a consulta respeita a autorização vigente.

### 17.3. Fluxo comum

1. Autenticar-se.
2. Acessar a home e criar/editar um rebanho ou iniciar uma simulação avulsa.
3. Preencher terreno, lote e saúde/manejo, com os mesmos campos para ambos os perfis.
4. Conferir os dados e a ocupação, sem exibir score ou resultado financeiro antes do cálculo definitivo.
5. Acionar **Gerar score** ou salvar o cadastro, conforme o fluxo já existente.
6. Validar e recalcular no servidor; salvar o histórico conforme as regras vigentes.
7. Exibir o relatório correspondente ao perfil autenticado.
8. Permitir editar os dados e recalcular ou iniciar uma nova simulação, respeitando as regras do fluxo utilizado.

Não criar formulário, motor de cálculo ou conjunto de premissas exclusivo para cooperativas. Não exigir uma etapa adicional somente por causa do perfil.

### 17.4. Informações ao final

A divisão abaixo define a proposta inicial de informações essenciais do pecuarista e o relatório completo da cooperativa.

| Informação | Pecuarista | Cooperativa |
| --- | --- | --- |
| Identificação do rebanho/propriedade, quando cadastrado, e data da análise | Sim | Sim |
| Score total e classificação de risco | Sim | Sim |
| Explicação resumida do resultado | Sim, em linguagem simples | Sim |
| Alertas prioritários e recomendações de manejo | Sim | Sim |
| Ocupação do pasto, capacidade e quantidade estimada de animais compatível com a área | Sim | Sim, com memória de cálculo e comparação com/sem rotação quando aplicável |
| Peso estimado de venda por animal | Sim | Sim |
| Custo total, capital inicial, receita, lucro/prejuízo e margem estimados | Sim, em resumo | Sim, com composição detalhada |
| Preço de equilíbrio | Sim, com explicação prática | Sim, com fórmula e valores utilizados |
| Cenário de queda de 10% no preço de venda | Resumo do impacto no lucro/prejuízo | Indicadores financeiros completos do cenário |
| Pontos e justificativas de cada componente do score | Não | Sim |
| GMD, mortalidade, animais vendidos e produção total estimados | Não, salvo alerta necessário à interpretação | Sim |
| Custos por item, doses, aplicações e embalagens | Não | Sim |
| Investimentos integrais, amortizações e parcelas atribuídas ao período | Não; mostrar aviso sobre despesas fora do capital inicial | Sim |
| Dados de entrada consolidados no relatório | Não; acessíveis no próprio formulário de edição | Sim |
| Fórmulas, parâmetros e memória de cálculo | Não | Sim |
| Identificação da versão das premissas | Sim | Sim, acompanhada dos valores utilizados |
| Avisos de estimativa, limites da análise e ausência de garantia de crédito/lucro | Sim | Sim |

O resumo do pecuarista deve permitir entender o resultado sem conhecimento técnico. Prejuízo, superlotação, risco sanitário e limitações relevantes nunca devem desaparecer por causa da simplificação. Os alertas podem citar o indicador necessário para explicar o risco, sem expor toda a memória de cálculo.

“Todas as informações” significa todas as informações da análise autorizada, e não acesso irrestrito a contas, rebanhos ou simulações de terceiros.

### 17.5. Propriedade dos dados e histórico

Nesta ampliação, ambos os perfis mantêm acesso somente aos próprios rebanhos e simulações. O perfil Cooperativa, por si só, não concede acesso aos dados de pecuaristas cadastrados em outras contas. A home continua aplicando os filtros de autoria da seção 16.

O relatório histórico utiliza as entradas, os resultados e as premissas salvos no cálculo original, sem recalcular com parâmetros atuais. O servidor entrega a representação adequada ao perfil autenticado. A administração mantém a consulta completa dos scores autorizada pela seção 14.

Vínculo entre cooperativa e cooperados, compartilhamento de análises e gestão de uma cooperativa com vários usuários não integram esta ampliação. Caso sejam necessários, exigirão definição própria de vínculo e permissões.

### 17.6. Implementação

- Manter um único caso de uso e motor de cálculo para ambos os perfis.
- Persistir o resultado completo para rastreabilidade e administração; aplicar a restrição na saída entregue ao usuário.
- Definir contratos de saída distintos para o resumo do Pecuarista e o relatório completo da Cooperativa, sem copiar regras de cálculo.
- Resolver a autorização na Aplicação e na Web, usando a identidade validada no servidor; o domínio de cálculo não depende de Identity.
- Não enviar detalhes restritos ao navegador do Pecuarista em JSON, HTML oculto, atributos, scripts ou respostas de endpoints. Ocultar elementos por CSS não constitui autorização.
- Aplicar as mesmas permissões a relatórios atuais e históricos e a eventuais exportações, caso sejam adicionadas posteriormente.
- Manter as proteções de autoria, concorrência e CSRF já especificadas.

### 17.7. Critérios de aceite adicionais

1. Pecuarista e Cooperativa concluem as mesmas etapas com os mesmos campos e validações.
2. Para entradas e premissas idênticas, o cálculo gera os mesmos resultados completos, independentemente do perfil.
3. O Pecuarista recebe todos os itens essenciais definidos na tabela e não recebe os detalhes exclusivos da Cooperativa no conteúdo da resposta.
4. A Cooperativa recebe o relatório completo de suas análises, com componentes do score, custos, fórmulas, entradas e premissas.
5. Requisições manipuladas não permitem obter o relatório completo com uma conta Pecuarista.
6. Cooperativas não conseguem consultar rebanhos ou scores de outras contas apenas por terem esse perfil.
7. O papel Administrador continua funcionando conforme a seção 14, sem ser concedido pelo cadastro público ou pela habilitação de Cooperativa.
8. Editar e recalcular preserva o perfil de visualização e gera novo histórico conforme as regras existentes.
9. Relatórios históricos mantêm os resultados originais e respeitam a autorização vigente no momento da consulta.
10. Testes de autorização verificam tanto o acesso direto aos endpoints quanto a ausência de detalhes restritos na resposta do Pecuarista.

## 18. Clean Code e organização do CRUD

Pecuarista e Cooperativa utilizam um CRUD compartilhado de rebanhos: criar, listar, consultar, atualizar e excluir. Os campos, as validações, a persistência e o cálculo são comuns; a autorização determina quais registros podem ser acessados e qual relatório pode ser entregue, conforme as seções 16 e 17.

A implementação deve aplicar Clean Code nas quatro camadas existentes:

- Nomes claros e vocabulário consistente com o negócio.
- Classes e métodos coesos, separando cálculo, orquestração, persistência, autorização e apresentação.
- Casos de uso organizados por funcionalidade e operação, sem serviços que concentrem todo o sistema.
- Regras e fórmulas compartilhadas, parâmetros identificados com suas unidades e ausência de duplicação entre perfis.
- Invariantes protegidas, erros esperados explícitos e fluxo de execução legível.
- SOLID aplicado conforme a necessidade, com contratos pequenos para dependências externas e sem abstrações especulativas.
- Testes focados no comportamento de negócio, nas permissões e na persistência atômica, preservados durante refatorações.

A revisão de código deve verificar essas diretrizes e as dependências entre camadas. Clean Code não altera fórmulas, premissas, permissões ou escopo aprovados. As orientações detalhadas de implementação estão em AGENTS.md.
