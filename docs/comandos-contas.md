# Comandos locais de contas

Implementam a atribuição explícita prevista nas seções 14 e 17 da SPEC. Exigem acesso de operador ao terminal do host e ao banco da aplicação. Não há endpoint HTTP, cadastro automático, senha especial ou concessão por variável de ambiente. O cadastro público continua criando apenas Pecuarista.

## Preparação

Use uma conta já cadastrada pela aplicação e o mesmo banco SQLite que contém essa conta. O modo de comando não aplica migrations nem inicia o servidor HTTP. O banco deve ter sido inicializado pela aplicação. Defina ConnectionStrings__Contas com caminho absoluto para evitar atingir outro arquivo por diferença de diretório de execução.

Exemplo PowerShell, a partir da raiz do repositório (substitua o caminho e o e-mail pelos desejados):

```powershell
$env:ConnectionStrings__Contas = 'Data Source=C:\caminho\da\aplicacao\contas.db'
dotnet run --project src/ScoreRebanho.Web --no-launch-profile -- contas ajuda
dotnet run --project src/ScoreRebanho.Web --no-launch-profile -- contas definir-perfil pessoa@example.com Cooperativa
dotnet run --project src/ScoreRebanho.Web --no-launch-profile -- contas conceder-administrador pessoa@example.com
```

Para voltar ao resumo do Pecuarista:

```powershell
dotnet run --project src/ScoreRebanho.Web --no-launch-profile -- contas definir-perfil pessoa@example.com Pecuarista
```

Em uma aplicação publicada, execute na pasta da publicação, com a mesma configuração:

```powershell
dotnet ScoreRebanho.Web.dll contas definir-perfil pessoa@example.com Cooperativa
dotnet ScoreRebanho.Web.dll contas conceder-administrador pessoa@example.com
```

O comando definir-perfil aceita exatamente Pecuarista ou Cooperativa. Administrador é um papel separado, atribuído somente pelo comando conceder-administrador. Repetir uma atribuição já existente retorna sucesso sem duplicar claims ou papéis. Não há comando de revogação administrativa neste incremento.

## Efeitos e decisões

- Definir perfil substitui o único claim perfil_negocio válido com UserManager.ReplaceClaimAsync; não altera papéis, credenciais ou históricos. Perfil vigente já é consultado no store a cada operação, inclusive com cookie antigo.
- Perfil ausente, desconhecido ou duplicado é recusado, sem reparação implícita. O operador deve revisar a inconsistência antes de usar o comando de perfil.
- Conceder Administrador cria esse papel se necessário e inclui a conta com RoleManager/UserManager. Não troca perfil nem cria exceção de autoria no CRUD. A conta deve sair e entrar novamente para atualizar os papéis do cookie.
- Contas inexistentes são recusadas antes de criar papéis. Não são cadastradas implicitamente.
- Falhas esperadas do Identity retornam mensagem simples e códigos técnicos no logging; defeitos inesperados são registrados como erro, sem sucesso aparente. Não se modifica senha nem se registram credenciais nos logs.
- A proteção do terminal e do arquivo SQLite corresponde à autorização operacional do host. Não foi inventado um novo papel de operador na aplicação para o bootstrap local.

Os comandos e sua interpretação ficam na Web, que já coordena Identity para autenticação e gestão de contas. EF Core continua na Infrastructure; Domain/Application não recebem dependências de Identity, console ou HTTP. Não há pacote ou migration novo.

## Saída e verificação

Código de saída 0 indica sucesso (incluindo atribuição já existente e ajuda); 2 indica sintaxe/perfil inválido; 3 indica conta não encontrada; 1 indica falha de atribuição, perfil cadastral inconsistente ou falha inesperada. Mensagens de falha vão para stderr; informações de diagnóstico seguem o logging configurado. Ajuda e sintaxe inválida terminam antes de abrir o banco.

Build sem avisos ou erros e 266 testes MSTest aprovados: 129 Domain, 53 Application, 13 Infrastructure e 71 Web. Foram adicionados 16 cenários: interpretação estrita, perfis conhecidos, argumentos/e-mails inválidos, idempotência, independência de perfil/papel, históricos inalterados, perfil vigente com cookie existente, ausência/duplicidade de perfil e recusa de contas inexistentes. Três cenários executam o DLL real em processo separado e verificam códigos de saída e persistência no SQLite isolado. Os testes não alteram contas reais.

A consulta administrativa global de scores foi implementada em [administracao-scores.md](administracao-scores.md). Preferência de tema implementada conforme [temas.md](temas.md). Inspeção visual e referência docs/ideias-prototipo-v6.md continuam pendentes.
