# Cadastro e acesso

A tela inicial direciona para /Conta/Cadastrar e /Conta/Entrar. O cadastro pede e-mail, senha e confirmação; após criar a conta, inicia uma sessão. Entrar valida as credenciais e permite manter a sessão. Sair é um POST autenticado com token CSRF.

## Organização

- Application: DTO de entrada, caso de uso CadastrarConta com FluentResults e contrato ICadastroContas.
- Infrastructure: stores Identity/EF Core, SQLite, migration inicial e gravação atômica de conta e claim de perfil.
- Web: ViewModels, perfil AutoMapper, controllers MVC, configuração de cookies e páginas Bootstrap.
- Domain: permanece independente de Identity e EF Core.

O cadastro público sempre atribui o claim perfil_negocio=Pecuarista. Não recebe perfil ou papéis do navegador e não concede Cooperativa nem Administrador.

## Decisões desta entrega

- E-mail identifica a conta; o índice normalizado é único, inclusive contra cadastros concorrentes.
- Senha de 8 a 128 caracteres, com número, maiúscula, minúscula e caractere especial. Validação e hash usam Identity.
- Cinco falhas de login bloqueiam a conta por cinco minutos.
- Sessões persistentes têm prazo de 14 dias com renovação; sem Manter conectado, o cookie é de sessão.
- Cookies HttpOnly e SameSite=Lax, com Secure obrigatório fora de Development.
- Sem confirmação ou recuperação por e-mail, conforme o escopo aprovado.
- Schema Identity versão 1, com chaves de login/token limitadas a 128, compatível com a migration do template oficial instalado do .NET 10. O produto utiliza senha; passkeys não integram esta entrega.
- Migration aplicada na inicialização, sem EnsureCreated e sem apagar dados existentes.
- AutoMapper 16.2.0 aceita a chave por configuração AutoMapper:LicenseKey (variável AutoMapper__LicenseKey); a licença de uso deve corresponder aos termos do fornecedor.

## Executar

Com acesso ao NuGet:

    dotnet restore ScoreRebanho.sln
    dotnet build ScoreRebanho.sln --no-restore
    dotnet test ScoreRebanho.sln --no-restore
    dotnet run --project src/ScoreRebanho.Web

A conexão fica em ConnectionStrings:Contas; o padrão é Data Source=contas.db. A primeira inicialização cria as tabelas via migration. Os testes Web substituem a conexão por um arquivo SQLite temporário exclusivo, removido após cada teste.

## Verificação e pendências

Foram adicionados testes MSTest da Application e de integração Web para cadastro, senha inválida, confirmação divergente, duplicidade, perfil fixo, hash, login, lockout, logout, CSRF, redirecionamento externo e mapeamento.

Neste ambiente, a restauração com APPDATA e cache de pacotes locais superou o erro de leitura da configuração do Windows, mas a conexão HTTPS ao NuGet foi bloqueada com NU1301. Build, execução das migrations, testes e inspeção visual em navegador dependem da restauração dos pacotes; não foram validados em execução.

Permanecem fora desta entrega: alteração de senha, comandos locais de habilitação de Cooperativa/Administrador, CRUD de rebanhos e relatórios. Esses itens continuam previstos na SPEC.

## Atualização da verificação

Com restauração autorizada e saída de compilação isolada em .local/verificacao, os 12 testes de integração Web e 6 testes da Application passaram, incluindo cadastro, autenticação, perfil fixo, migration e logout. A limitação de NuGet registrada anteriormente foi superada. A inspeção visual em navegador permanece pendente.
