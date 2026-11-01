# CRUD e autorização por perfil

## Casos de uso

A Application possui CriarRebanho, ListarRebanhos, ConsultarRebanho, ConsultarEdicaoRebanho, AtualizarRebanho e ExcluirRebanho. Cada operação recebe seu DTO e retorna FluentResults. Os DTOs não contêm autoria, perfil ou resultados calculados como entradas.

IUsuarioAtual fornece somente identificador e perfil de negócio da conta autenticada. AcessoRebanhos aplica o filtro de autoria antes de consultar dados, editar ou excluir. Criação e atualização usam o mesmo CalcularScoreRebanho, validam a identificação no Domain e passam o resultado completo ao repositório para gravação atômica. Versão desatualizada é recusada; conflitos ou desaparecimento do registro durante a gravação são traduzidos para falhas esperadas.

Listar usa paginação e autoria do repositório existente. Consultar e consultar dados de edição não recalculam. O contrato de edição contém apenas identificação, versão e entradas; não leva resultados restritos ao formulário.

Códigos estáveis de erro: NAO_AUTENTICADO, PERFIL_INVALIDO, NAO_ENCONTRADO, CONFLITO e ENTRADA_INVALIDA. Falhas do motor preservam seus códigos numéricos. Falhas inesperadas continuam como exceções para o tratamento global.

## Relatórios

ResumoPecuaristaDto inclui identificação no envelope, data, versão das premissas, score/classificação, explicação, alertas/recomendações, capacidade/ocupação, animais compatíveis, peso individual de venda, resumo financeiro, preço de equilíbrio com explicação prática e lucro no cenário adverso. Seus avisos destacam estimativa, ausência de garantia e despesas fora do capital inicial.

Esse tipo não contém entradas consolidadas, componentes do score, GMD, mortalidade, produção total, custos por item, produtos, doses, embalagens, parâmetros numéricos ou fórmulas. RelatorioCooperativaDto contém a análise completa. A discriminação JSON preserva o tipo realmente autorizado pelo servidor; não é um parâmetro de entrada nem uma permissão fornecida pelo usuário.

ConsultarHistoricoProprio aplica a mesma autorização vigente sobre o snapshot original, mesmo após excluir o rebanho. O papel Administrador não altera essa interface; sua consulta administrativa global será um caso de uso separado.

A memória textual das fórmulas passa a ser registrada no snapshot no momento do cálculo. Ela não é gerada novamente ao consultar o histórico. A adição é compatível com o formato JSON 1: snapshots anteriores sem esse campo continuam legíveis e não recebem fórmulas atuais inventadas.

## Etapa MVC implementada

As rotas e telas compartilhadas foram implementadas com ViewModels, AutoMapper, FluentResults, POST, CSRF e confirmação de exclusão. Os testes HTTP verificam autoria e conteúdo autorizado. Consulte [telas-crud.md](telas-crud.md) para rotas e verificações.

A referência docs/ideias-prototipo-v6.md continua ausente. Ficam pendentes também comandos locais de Cooperativa/Administrador, alteração de senha e administração global de scores.

## Resolvedor da Web

UsuarioAtual usa o principal autenticado para localizar a conta no Identity e busca os claims de perfil no store a cada operação. Um cookie com claim antigo de Cooperativa não mantém esse acesso após a conta voltar a Pecuarista. Contas inexistentes ou perfis ausentes, ambíguos ou desconhecidos não obtêm acesso pelo contrato. Administrador não substitui perfil de negócio nem autoria.

Verificação atual: 230 testes aprovados e build sem avisos/erros. A integração da Web cobre resolução de identidade, chamadas dos casos de uso e endpoints HTTP/HTML com Identity/SQLite isolados.
