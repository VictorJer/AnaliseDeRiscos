# Telas MVC do CRUD compartilhado

Após entrar, a raiz redireciona para /Rebanhos. A home pública mantém a apresentação e os botões Cadastrar e Entrar. Pecuarista e Cooperativa usam o mesmo formulário Bootstrap, controller, casos de uso e validações.

## Rotas

- GET /Rebanhos: registros próprios, 20 por página, atualização mais recente primeiro.
- GET/POST /Rebanhos/Cadastrar: identificação, terreno, lote e manejo; salvar gera o score e o histórico atomicamente.
- GET /Rebanhos/Detalhes/{id}: relatório do último resultado, sem recalcular.
- GET/POST /Rebanhos/Editar/{id}: entradas preenchidas e versão de concorrência; salvar gera novo histórico.
- POST /Rebanhos/ConferirCadastro e /Rebanhos/ConferirEdicao/{id}: ocupação no servidor, sem persistência ou pontuação.
- GET/POST /Rebanhos/Excluir/{id}: confirmação e exclusão do cadastro, preservando os históricos.
- GET /Historico/Detalhes/{id}: snapshot original com a representação permitida pelo perfil vigente.

Todas as rotas de dados exigem autenticação. Escritas e conferências exigem antiforgery. Consultas, edição e exclusão de IDs alheios retornam 404, inclusive para administradores nesta interface. Conflitos retornam 409, preservam o formulário e oferecem recarregar os dados. Respostas de dados usam NoStore.

## Formulário e relatórios

ViewModels são exclusivos da Web; AutoMapper faz a conversão para DTOs. FluentResults é traduzido em mensagens de formulário e respostas HTTP. Controllers não calculam indicadores.

O formulário funciona sem JavaScript, com campos agrupados e duas ações: conferir ocupação ou salvar e gerar score. A cultura pt-BR aceita decimais com vírgula; moeda e percentuais são formatados somente na apresentação. Arrendamento é obrigatório para terreno arrendado; rotação exige piquetes. Valores iniciais de interface: pasto pronto e 180 dias, editáveis pelo usuário e sujeitos à validação do servidor.

Relatórios têm ViewModels e Razor Views distintos. O resumo Pecuarista inclui os indicadores essenciais e alertas da SPEC 17; a Cooperativa recebe também composição do score, produção, custos, produtos/doses, entradas, parâmetros e fórmulas originais. A escolha é feita pela representação autorizada pela Application, nunca por parâmetros HTTP. Informações restritas não são embutidas no HTML do resumo.

## Verificação

Build da solução em .local/verificacao concluído sem avisos/erros. Suíte MSTest completa: 230 aprovados (129 Domain, 48 Application, 13 Infrastructure e 40 Web).

Testes HTTP usam Identity e SQLite isolados: fluxo completo para ambos os perfis, CSRF, acessos diretos/IDs alheios, administrador sem exceção de autoria, manipulação de entradas/perfil, mudança do perfil com cookie existente, ausência de dados restritos, decimais pt-BR, paginação, validações condicionais, concorrência, consultas/conferência sem escrita e históricos após exclusão. Configuração AutoMapper também validada.

A inspeção visual não foi concluída: o Computer Use encerrou a automação por não conseguir confirmar a URL do navegador. O Bootstrap foi servido com sucesso nos testes HTTP; isso não equivale a validar a aparência em navegador.

## Pendências de outras etapas

O fluxo de simulação avulsa foi implementado no incremento seguinte, documentado em [simulacoes-avulsas.md](simulacoes-avulsas.md). Alteração de senha implementada conforme [autenticacao.md](autenticacao.md). Continuam pendentes comandos locais de atribuição de Cooperativa/Administrador, consulta administrativa global e preferência de tema. A referência docs/ideias-prototipo-v6.md permanece ausente; a ordem visual do protótipo precisa ser conferida quando disponível. Fórmulas e permissões aprovadas não foram alteradas.
