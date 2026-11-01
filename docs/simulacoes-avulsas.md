# Simulações avulsas

O fluxo autenticado permite calcular sem nome/propriedade e sem criar um cadastro de rebanho. A navegação oferece Nova simulação; os relatórios oferecem Novo score, que abre entradas limpas (mantendo apenas os valores iniciais de interface já usados no CRUD).

## Casos de uso e persistência

GerarSimulacao recebe apenas entradas, valida a conta e usa CalcularScoreRebanho. Somente cálculos válidos são salvos por IRepositorioScores.SalvarAvulsoAsync; o autor é resolvido no servidor. O histórico fica sem origem de rebanho, nome ou propriedade. O contrato retorna apenas o identificador salvo; o relatório é consultado por ConsultarHistoricoProprio, respeitando o perfil vigente.

ConsultarEdicaoSimulacao carrega as entradas normalizadas do snapshot próprio, sem recalcular. Editar e recalcular cria outro histórico e nunca altera o original. A edição avulsa recusa históricos originados do CRUD: rebanhos seguem pelo formulário de edição do próprio cadastro. Essa separação mantém explícita a ação escolhida e evita criar uma cópia avulsa acidentalmente.

Não há novo armazenamento de sessão, migration, fórmula ou premissa. Falhas esperadas usam FluentResults; cancelamento e falhas inesperadas continuam propagados. Não se cria cadastro de rebanho a partir de uma simulação avulsa.

## MVC e reutilização

- GET /Simulacoes/Nova abre o formulário.
- GET /Simulacoes/Editar/{id} preenche entradas da simulação própria.
- POST /Simulacoes/Conferir calcula somente ocupação, sem gravação.
- POST /Simulacoes/Gerar salva um novo cálculo e redireciona para /Historico/Detalhes/{id}.

Todas as ações exigem autenticação; os POSTs exigem antiforgery. Consultas usam NoStore. IDs alheios retornam 404, inclusive para administradores nesta interface.

SimulacaoFormularioViewModel é exclusivo da Web e mapeado por SimulacoesProfile. O EditorTemplate SimulacaoViewModel compartilha campos e validações com o CRUD, preservando o prefixo Entradas. A conferência usa a mesma partial e caso de uso. O formulário funciona sem JavaScript e aceita vírgula decimal. Relatórios reutilizam as representações já autorizadas por perfil, sem entregar composição restrita ao Pecuarista.

## Verificação e pendências

Build sem avisos ou erros. Suíte completa: 241 testes aprovados (129 Domain, 53 Application, 13 Infrastructure e 46 Web). Os novos testes verificam geração/recálculo para ambos os perfis, snapshot original imutável, ausência de cadastro de rebanho, edição própria, inputs inválidos, conferência sem persistência, CSRF, autenticação, cancelamento e recusa de acesso alheio, incluindo administrador. Os testes anteriores do CRUD também continuam aprovados após a extração dos campos.

Alteração de senha implementada conforme [autenticacao.md](autenticacao.md). Atribuição local implementada conforme [comandos-contas.md](comandos-contas.md). Consulta administrativa implementada conforme [administracao-scores.md](administracao-scores.md). Preferência de tema implementada conforme [temas.md](temas.md). Ainda pendente: validação visual em navegador. O documento docs/ideias-prototipo-v6.md continua ausente.
