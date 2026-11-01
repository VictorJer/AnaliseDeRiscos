# AGENTS.md — Instruções para agentes de código

## Contexto do projeto

Este repositório implementa o **Score do Rebanho**, uma aplicação web ASP.NET Core MVC em **.NET 10** e C#. Antes de alterar código, leia `SPEC.md`. Ela define o escopo do MVP, o fluxo do usuário, os cálculos, as premissas e os critérios de aceite.

Se estas instruções e `SPEC.md` parecerem divergir, preserve as regras de arquitetura deste arquivo e sinalize a divergência de produto. Não altere silenciosamente fórmulas ou premissas. Para regras ambíguas, registre a decisão necessária e não invente uma regra de negócio sem deixar explícita a escolha.

## Arquitetura obrigatória

A solução usa quatro camadas/projetos:

```text
ScoreRebanho.Domain
ScoreRebanho.Application
ScoreRebanho.Infrastructure
ScoreRebanho.Web
```

Dependências permitidas:

```text
Domain          → nenhuma camada da aplicação
Application     → Domain
Infrastructure  → Application + Domain
Web             → Application
Web (composition root) configura Infrastructure
```

- **Domain** contém conceitos e regras centrais do negócio. Não referencia ASP.NET Core, Entity Framework, AutoMapper, FluentResults ou infraestrutura.
- **Application** contém casos de uso, DTOs, contratos de serviços e orquestração. Não referencia tipos de apresentação (ViewModels) nem tipos HTTP.
- **Infrastructure** implementa contratos técnicos definidos pela Application e é responsável pela persistência com **Entity Framework Core** (EF Core), incluindo `DbContext`, configurações de mapeamento e implementações de repositório. Também configura os stores do ASP.NET Core Identity usando EF Core. Use SQLite, conforme a SPEC vigente. Não referencie tipos do EF Core nem do Identity no Domain; não exponha `DbSet`, `IQueryable` ou entidades de persistência para Web/Application.
- **Web** contém controllers MVC, ViewModels, Razor Views, validação de apresentação, autenticação/autorização e configuração/composição de dependências. As Views usam Bootstrap para layout e componentes responsivos.

Não mova lógica de negócio para controllers, ViewModels, Razor Views ou JavaScript. Não faça a camada Domain depender de Application ou Infrastructure.

## Fluxo MVC, ViewModels, DTOs e serviços

- A ação do controller recebe um **ViewModel** (`VM`) apropriado à tela. Não receba DTOs de Application diretamente como parâmetros HTTP.
- Use **AutoMapper** para mapear ViewModels para DTOs de entrada da Application e DTOs de saída para ViewModels de resposta.
- Quando o controller solicitar um serviço/caso de uso, passe um **DTO**; não passe o ViewModel, `IFormCollection`, `HttpContext` ou `ModelState` para Application.
- Controllers coordenam HTTP: validam `ModelState`, mapeiam, chamam o serviço e escolhem a View/redirect. Não calculam score, custos ou regras de domínio.
- ViewModels pertencem ao projeto Web e devem ser desenhados para a interface. DTOs pertencem à Application e devem representar a entrada/saída de um caso de uso. Não reutilize um tipo nas duas camadas.
- Crie perfis AutoMapper explícitos e agrupados por funcionalidade. Mapeamentos importantes devem ser verificáveis; não esconda conversões com regras de negócio dentro de `ForMember`.
- Não mapeie valores calculados enviados pelo navegador como fonte confiável. A Application/Domain recalcula os resultados no servidor a partir das entradas válidas.

Fluxo de referência:

```text
HTTP form → Controller recebe VM → AutoMapper → DTO de entrada
          → Serviço da Application → Domain/cálculos
          → FluentResults Result<DTO de saída> → AutoMapper → VM de resultado
          → View Razor
```

## FluentResults e tratamento de erros

- Use **FluentResults** no limite dos serviços/casos de uso da Application para comunicar sucesso ou falha sem exceções como fluxo normal.
- Serviço de Application retorna `Result` ou `Result<TDto>` conforme exista payload de saída.
- Use `Result.Fail(...)` para falhas esperadas, como entrada inválida segundo regras da aplicação ou impossibilidade de calcular um resultado. Inclua mensagens claras e, quando útil, tipos/metadados de erro estáveis para que a Web decida como apresentá-los.
- Não exponha exceções internas, stack traces, nomes de tabela ou detalhes sensíveis ao usuário. Registre detalhes técnicos no mecanismo de logging apropriado.
- A camada Web traduz falhas para `ModelState`/mensagens de validação ou uma página de erro adequada; mensagens de domínio devem ser apresentadas em linguagem simples.
- Não use `Result` para esconder defeitos de programação ou falhas inesperadas. Exceções inesperadas devem seguir o tratamento global de erros e logging.
- Evite propagar `FluentResults` até Domain ou Infrastructure sem necessidade. Mantenha-o no contrato da Application e nos consumidores imediatos.

## AutoMapper e dependências

- Registre perfis AutoMapper e serviços da Application no composition root da aplicação Web, usando injeção de dependência.
- Mantenha referências e pacotes no projeto que realmente os utiliza: AutoMapper nos projetos que definem/consomem perfis; FluentResults na Application e no projeto Web que interpreta resultados.
- Use **ASP.NET Core Identity** para autenticação e gestão de contas. Configure serviços e cookies na Web e stores persistidos por EF Core na Infrastructure. Mantenha tipos de usuário/Identity fora do Domain e não passe `IdentityUser` ou tipos de Identity aos casos de uso de negócio; se Application precisar identificar o usuário, passe um identificador/contrato simples.
- Use migrations do EF Core para evoluir o schema incluindo tabelas do Identity. Não armazene senhas manualmente nem implemente autenticação própria.
- Não invente papéis, permissões ou regras de autorização; implemente apenas as necessárias e especificadas para cada funcionalidade.
- Não adicione pacote ou abstração adicional sem necessidade concreta. Confira a versão e compatibilidade alvo antes de adicionar dependências.

## Regras de negócio e premissas

- Centralize valores de referência e fórmulas; nunca os espalhe por controllers, views ou perfis de mapeamento.
- Preserve precisão durante os cálculos; arredonde para apresentação conforme a SPEC.
- Deixe explícitas unidades e conversões (kg, UA, hectares, dias, arrobas, R$).
- Proteja divisões e casos extremos, aplique limites definidos e produza resultados determinísticos.
- Registre premissas e decisões de implementação. Se for necessário escolher uma interpretação para uma ambiguidade da SPEC, mantenha-a localizada, documentada e coberta por testes.
- Score é uma estimativa explicável. Não prometa lucro, segurança ou aprovação de crédito.

## Estilo de implementação

- Use C# e convenções idiomáticas do .NET, nomes claros e nullable reference types quando habilitado no projeto.
- Mantenha controllers enxutos, métodos coesos e responsabilidades claras.
- Prefira `decimal` para valores monetários e cálculos financeiros; use tipos inteiros para contagens e dias.
- Use ASP.NET Core Identity para autenticação. Use Entity Framework Core na Infrastructure para persistência, incluindo os stores do Identity, e mantenha o Domain independente dessas tecnologias. Não introduza APIs externas ou arquitetura distribuída fora do escopo sem solicitação explícita.
- Mantenha interface e mensagens voltadas ao usuário em português do Brasil, salvo pedido diferente.
- Use Bootstrap nas páginas Razor para grid, formulários, botões, alertas, tabelas e demais componentes visuais. Prefira classes e componentes existentes do Bootstrap antes de criar CSS próprio; mantenha CSS customizado pequeno e específico do projeto.
- Não adicione comentários que apenas repitam o código; documente decisões de negócio que não sejam óbvias.

## Testes automatizados

- Use **MSTest** em todos os projetos de teste. Não introduza xUnit, NUnit ou outra framework de testes.
- Organize os projetos de teste por camada, por exemplo `ScoreRebanho.Domain.Tests` e `ScoreRebanho.Application.Tests`, usando os pacotes e SDK oficiais do MSTest compatíveis com `net10.0`.
- Escreva testes com `[TestClass]` e `[TestMethod]` (ou os atributos equivalentes suportados pela versão configurada) e use as asserções do MSTest.
- Priorize testes unitários determinísticos para regras de domínio e casos de uso, incluindo limites, entradas inválidas e os cenários de cálculo definidos na `SPEC.md` após resolver as ambiguidades.
- Mantenha testes independentes entre si; não dependa de ordem de execução, serviços externos ou dados reais mutáveis.
- Execute `dotnet test` na solução (ou no projeto afetado quando apropriado) e informe o resultado real.

## Verificação de mudanças

- Ao implementar ou alterar uma regra, adicione ou atualize testes automatizados de Domain/Application cobrindo comportamento normal, limites e casos extremos relevantes.
- Antes de concluir alterações de código, execute os comandos de build e testes apropriados à solução existente. Se ainda não houver solução ou SDK disponível, informe isso em vez de alegar sucesso.
- Não diga que algo foi testado ou compilado se o comando correspondente não foi executado com sucesso.
- Revise mudanças para confirmar que referências entre camadas continuam respeitando a arquitetura e que ViewModels não atravessam o limite da Application.

## Comunicação de conclusão

Ao finalizar uma tarefa, resuma:

1. O que foi alterado e por quê.
2. Quais verificações foram executadas e seus resultados.
3. Quais decisões ou pendências da SPEC continuam abertas.
## Clean Code e manutenção

- Use nomes que expressem intenção, com vocabulário consistente para rebanho, simulação, score, custo e relatório. Evite abreviações obscuras e nomes genéricos como Manager, Helper ou Utils sem responsabilidade definida.
- Mantenha classes e métodos coesos, com uma responsabilidade clara. Separe cálculo, persistência, autorização e apresentação. Extraia métodos quando isso esclarecer uma operação; não imponha limites arbitrários de linhas.
- Organize casos de uso da Application por funcionalidade e operação: criar, consultar, listar, atualizar e excluir rebanhos. Evite um serviço central que concentre responsabilidades sem relação.
- Aplique SOLID com necessidade concreta: injete dependências externas por contratos pequenos e preserve as dependências entre camadas. Não crie uma interface para cada classe nem hierarquias para extensões hipotéticas.
- Compartilhe regras de negócio e validações de domínio; não duplique fórmulas entre casos de uso ou perfis. Validações de apresentação podem complementar as regras autoritativas do servidor.
- Use cláusulas de guarda e fluxo legível. Evite aninhamentos excessivos, parâmetros booleanos que selecionem operações distintas e efeitos colaterais inesperados.
- Substitua números mágicos por parâmetros ou constantes com nomes e unidades. Preserve os valores aprovados e a versão das premissas.
- Proteja invariantes e encapsule alterações de estado. Não exponha coleções mutáveis ou setters indiscriminados quando permitirem estados inválidos.
- Use operações assíncronas para acesso a dados e propague CancellationToken quando aplicável. Não envolva cálculos puros em Task.Run por padrão.
- Remova código morto e dependências sem uso. Comentários explicam decisões e convenções; os nomes e a estrutura devem explicar a execução.
- Prefira a solução mais simples que cumpra a SPEC. Não introduza CQRS, MediatR, repositório genérico, microserviços ou padrões adicionais somente por estilo.
- Refatore preservando o comportamento observável e os contratos de autorização. Testes devem verificar resultados e regras, sem depender de métodos privados ou da organização interna das classes.

## CRUD compartilhado e relatórios por perfil

- Pecuarista e Cooperativa utilizam o mesmo CRUD de rebanhos, os mesmos campos, validações e motor de cálculo. Não duplique controllers, entidades, tabelas ou serviços somente por causa do perfil.
- Criar e atualizar salvam rebanho e novo score histórico atomicamente. Consultar não recalcula. Excluir exige confirmação, POST e proteção CSRF, preservando os scores históricos.
- Resolva autoria e perfil com base na identidade autenticada no servidor. Não aceite autor, perfil ou permissões enviados pelo cliente como fonte confiável.
- A Application aplica as regras de acesso por contratos simples, sem dependência de HttpContext ou tipos do Identity. A Web protege as rotas e apresenta os resultados autorizados.
- Mantenha saídas distintas para o resumo do Pecuarista e o relatório completo da Cooperativa, conforme a seção 17 da SPEC. Nunca envie informações restritas em HTML oculto, JSON, atributos ou scripts.
- Cada usuário acessa apenas seus registros no CRUD, inclusive administradores nessa interface. Cooperativa não implica acesso a cooperados ou administração global.
- O papel Administrador é independente dos perfis de negócio e mantém somente as permissões administrativas especificadas.
- Relatórios históricos preservam entradas, resultados e premissas originais; a representação entregue respeita a autorização vigente.
- Inclua testes MSTest de autorização na Application e testes de integração na Web para autoria, acesso direto a endpoints e ausência de detalhes restritos nas respostas ao Pecuarista. Use banco de teste isolado quando necessário, nunca dados reais.
