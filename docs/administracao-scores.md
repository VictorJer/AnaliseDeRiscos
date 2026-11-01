# Administração de scores

A área /AdministracaoScores permite ao Administrador consultar scores de todos os autores, conforme a SPEC 14. O menu aparece para sessões com o papel Administrador. Para conceder esse papel a uma conta existente, consulte [comandos-contas.md](comandos-contas.md) e entre novamente na conta.

## Rotas e comportamento

- GET /AdministracaoScores lista 20 históricos por página, ordenados por data UTC decrescente e identificador decrescente para desempatar datas iguais.
- GET /AdministracaoScores/Detalhes/{id} abre o snapshot completo: entradas, indicadores, composição do score, custos, produtos, premissas e fórmulas originais.

A lista identifica autor, rebanho/propriedade (ou simulação avulsa), data, pontuação/classificação e versão das premissas. O e-mail do autor vem da conta vigente, enquanto o identificador de autoria, nome/propriedade e dados do cálculo vêm do histórico. Essa identificação não altera o snapshot original.

A área contém somente consultas. Não há ações administrativas de editar, recalcular ou excluir históricos. Relatórios continuam disponíveis após excluir o cadastro de rebanho. As consultas não chamam a calculadora nem geram histórico. Página inválida retorna 400; identificador ausente retorna 404; páginas além do total ficam vazias.

## Autorização

Web exige o papel Identity Administrador e autenticação. A Application também valida a conta e o indicador administrativo obtido pelo resolvedor de identidade. UsuarioAtual consulta o papel vigente no store a cada operação: retirar o papel bloqueia o acesso administrativo mesmo com cookie antigo. A concessão local exige novo login para que o cookie passe na proteção de rota.

Administrador é independente do perfil Pecuarista/Cooperativa. O contrato simples ContaAutenticada contém o indicador administrativo, sem tipos Identity ou HTTP na Application. O indicador nunca é preenchido por formulário ou parâmetros de URL. Conta sem papel recebe NAO_AUTORIZADO; conta ausente recebe NAO_AUTENTICADO. A Web traduz essas falhas em Forbid/Challenge; cookies redirecionam às páginas de acesso negado ou login.

Os casos de uso do CRUD e da consulta histórica própria continuam filtrando autoria. Um Administrador com perfil Pecuarista recebe o resumo na interface comum, e o relatório completo somente pela rota administrativa autorizada. Cooperativa não concede administração global. Não foi criada exceção de autoria para editar/excluir registros alheios.

## Implementação

Application possui ListarScoresAdministrativos, ConsultarScoreAdministrativo e o contrato somente leitura IConsultaScoresAdministrativos. Infrastructure implementa consultas EF Core sem tracking; a listagem projeta apenas metadados, sem carregar os JSONs completos. O índice global existente por data/ID é reutilizado, sem migration ou mudança de schema. Paginação protege deslocamentos maiores que int.MaxValue.

Web recebe ViewModels, mapeia com AdministracaoScoresProfile e apresenta páginas Bootstrap com NoStore. O corpo da análise completa foi extraído para uma partial compartilhada com o relatório da Cooperativa. Cabeçalhos e ações de navegação permanecem distintos; a extração não muda cálculos ou dados autorizados do resumo Pecuarista.

## Verificação e pendências

Build sem avisos/erros e 284 testes aprovados: 129 Domain, 61 Application, 16 Infrastructure e 78 Web. Os 18 novos cenários verificam autorização na Application, independência do perfil, cancelamento, preservação do snapshot, paginação/ordenação/desempate, autores distintos, histórico após exclusão do cadastro, acessos diretos HTTP, ausência de ações de escrita, perfil sem papel, remoção do papel com cookie existente e configuração AutoMapper. As suítes anteriores também passaram. Os testes usam SQLite isolado; não foram alterados banco ou contas reais.

A aparência ainda precisa de inspeção visual em navegador. Preferência de tema e a referência docs/ideias-prototipo-v6.md continuam pendentes. Nenhuma fórmula ou premissa de cálculo foi alterada.
