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

## Incremento 7 — alertas e conjunto de premissas

PremissasAnalise reúne as cinco partes da configuração e recusa versões divergentes ou parâmetros inválidos. AvaliadorRiscos gera códigos, explicações e recomendações determinísticas para prejuízo, superlotação, ausências sanitárias, margem baixa, prejuízo no cenário adverso, subocupação e rotação com piquetes insuficientes. Os limites reutilizam as premissas existentes; não foram alteradas fórmulas.

Decisão localizada de apresentação dos alertas: prejuízo, superlotação, vacina/carrapato/vermífugo, margem, cenário adverso, subocupação e rotação. Essa ordem está coberta por testes. A referência docs/ideias-prototipo-v6.md está ausente, portanto a ordem visual detalhada do protótipo ainda deve ser conferida quando ela estiver disponível.

Verificação: 126 testes de domínio passaram, incluindo 10 cenários de alertas.

## Incremento 8 — caso de uso completo da análise

CalcularScoreRebanho recebe um DTO imutável contendo somente entradas, constrói os valores de domínio e executa produção, custos, financeiro, score e alertas no servidor. O resultado interno completo reúne DTOs de indicadores, entradas normalizadas, data UTC via TimeProvider, versão, valores de todas as premissas e convenções de arredondamento/aplicações. A resposta é serializável para o futuro histórico; não recebe valores calculados, autoria ou perfil do cliente.

Entrada inválida retorna FluentResults com Codigo=ENTRADA_INVALIDA e Campo. Estouro ou divisão numérica impossível retorna LIMITE_NUMERICO; margem indisponível retorna MARGEM_INDISPONIVEL. Falhas não entregam análise parcial. A captura de ArgumentException fica restrita à construção das entradas; configuração inválida e defeitos inesperados não são convertidos em erros normais.

O conjunto de premissas usa objetos de valor de domínio imutáveis dentro do DTO de snapshot; não contém EF Core, Identity ou tipos de apresentação. Limites de classificação e convenções também são copiados como valores para rastreabilidade. O registro de DI ocorre no composition root e valida a configuração na inicialização.

Verificação: build da solução sem avisos ou erros e 157 testes aprovados (126 Domain, 19 Application, 12 Web). Foram verificados totais integrados, precisão, alertas, serialização/recuperação de entradas e premissas, configuração personalizada, coleções somente leitura, falhas numéricas e propagação de defeitos inesperados.

Não foi adicionado endpoint de cálculo nesta etapa. A análise completa é um contrato interno para persistência e composição; a exposição HTTP dependerá de autoria e projeções distintas por perfil. Próximo incremento: entidades/contratos de persistência de rebanhos e scores históricos, migrations e transação atômica. As telas CRUD e relatórios por perfil continuam pendentes. Nenhuma fórmula ou permissão aprovada foi alterada.

## Incremento 9 — modelo do rebanho e contratos de persistência

Rebanho encapsula autoria, identificação, último score, datas UTC e versão de concorrência. Criar gera identificadores; atualizar troca o score e a versão, preservando autor e data de criação. As entradas e o resultado ficam no snapshot completo apontado pelo cadastro, evitando duas cópias divergentes da mesma análise.

Application define IRepositorioRebanhos e IRepositorioScores com operações assíncronas e CancellationToken, sem DbSet, IQueryable, EF Core ou Identity. As saídas são DTOs internos; os estados esperados de escrita são sucesso, não encontrado e conflito. A futura orquestração da Application os traduzirá para FluentResults e aplicará autoria/perfil obtidos no servidor.

Verificação: 129 testes de domínio aprovados, incluindo criação, atualização e proteção de autoria/versão. A autorização por perfil e as ações MVC continuam pendentes.

## Incremento 10 — EF Core, SQLite e histórico atômico

Implementados repositórios técnicos de rebanhos e histórico, mapeamentos e migration PersistirRebanhosEScores. Criar/atualizar salvam cadastro e análise completa na mesma transação; versões desatualizadas são recusadas. Excluir mantém scores imutáveis, inclusive nome/propriedade originais. A listagem filtra autor, pagina em 20 e ordena por atualização. Histórico avulso permanece separado de rebanhos.

Decisões técnicas e comandos de migrations estão em docs/persistencia.md: snapshot JSON versionado, datas em ticks UTC, vínculo composto do último score com o autor, origem histórica sem FK ao cadastro, token GUID também na exclusão e proteção de histórico por DbContext/triggers.

Verificação: build da solução sem avisos ou erros e 173 testes aprovados (129 Domain, 19 Application, 13 Infrastructure, 12 Web). Migrations foram testadas somente em bancos isolados, inclusive preservando contas Identity existentes. O banco real da aplicação aberta não foi alterado nesta execução.

Próximo incremento: casos de uso CRUD na Application, com identidade confiável, FluentResults e testes de autorização; depois telas e relatórios por perfil. A referência visual docs/ideias-prototipo-v6.md continua ausente.

## Incremento 11 — casos de uso CRUD e relatórios autorizados

Application implementa criar, listar, consultar, consultar dados de edição, atualizar e excluir com IUsuarioAtual, autoria no servidor e FluentResults. Autorização ocorre antes de validar/recalcular registros existentes; IDs alheios retornam não encontrado. Os perfis compartilham o mesmo motor e a mesma persistência. Entradas inválidas ou versões obsoletas não gravam novos históricos.

ResumoPecuaristaDto e RelatorioCooperativaDto possuem contratos distintos, inclusive na serialização. A consulta de histórico próprio usa o perfil vigente sobre o snapshot original. Fórmulas textuais são armazenadas no momento do cálculo; snapshots anteriores sem esse campo continuam legíveis sem gerar memória atual retroativamente.

Verificação: 45 testes de Application aprovados, cobrindo autoria, ambos os perfis, payloads com campos indevidos, validações, concorrência, cancelamento, propagação de falhas inesperadas e ausência de dados restritos no JSON atual/histórico. Os 129 testes Domain e 13 testes Infrastructure também passaram. Detalhes em docs/crud-autorizacao.md.

## Incremento 12 — identidade vigente na Web

UsuarioAtual implementa o contrato da Application usando o principal autenticado apenas para identificar a conta. A existência do usuário e seu perfil ativo são lidos do Identity; claims antigos do cookie não escolhem a abrangência do relatório. Ausência ou duplicidade de perfis é recusada, sem conceder Cooperativa por fallback. O papel Administrador continua independente do perfil de negócio e não cria exceção de autoria no CRUD.

Os casos de uso, o resolvedor de identidade e o contrato IUsuarioAtual foram registrados no composition root. Testes com Identity e SQLite isolado verificam mudanças de perfil, conta inexistente, perfis ambíguos e administrador tentando consultar/editar/excluir o rebanho de outro autor.

Verificação final: build da solução sem avisos ou erros e 203 testes aprovados (129 Domain, 45 Application, 13 Infrastructure e 16 Web). Os testes novos da Web exercitam o resolvedor e os serviços integrados; ainda não existem endpoints CRUD de rebanhos para testar respostas HTTP/HTML. Essa cobertura será adicionada junto às telas MVC, POST, CSRF e confirmação de exclusão.

## Incremento 13 — telas MVC e integração HTTP

Implementadas listagem, cadastro, conferência da ocupação, edição, confirmação de exclusão e relatórios atuais/históricos por perfil. Documentação: [telas-crud.md](telas-crud.md) e [conferencia-ocupacao.md](conferencia-ocupacao.md). Build sem avisos/erros e 230 testes aprovados. A inspeção visual foi bloqueada pelo Computer Use; não foi declarada validada. As pendências externas a este CRUD estão registradas no documento das telas.

## Incremento 14 — simulações avulsas

Implementados geração de score sem cadastro de rebanho, conferência, edição das entradas e recálculo em novo histórico. O formulário compartilha EditorTemplate com o CRUD e os relatórios respeitam o perfil vigente. Detalhes e decisão sobre edição exclusivamente avulsa em [simulacoes-avulsas.md](simulacoes-avulsas.md). Verificação: build sem avisos/erros e 241 testes aprovados (129 Domain, 53 Application, 13 Infrastructure e 46 Web). Permanecem as pendências administrativas, senha, tema e inspeção visual.

## Incremento 15 — alteração de senha

Implementada a tela autenticada com senha atual, nova senha e confirmação, usando UserManager.ChangePasswordAsync e renovação da sessão via Identity. Sem mudanças de schema ou regras de cálculo. Detalhes em [autenticacao.md](autenticacao.md). Build sem avisos/erros e 250 testes aprovados. Permanecem pendentes comandos locais de Cooperativa/Administrador, consulta administrativa global, tema e inspeção visual; a referência do protótipo continua ausente.

## Incremento 16 — comandos locais de contas

Implementados comandos explícitos para definir Pecuarista/Cooperativa e conceder Administrador a conta existente. Usam Identity, preservam a separação entre perfil e papel e não iniciam HTTP nem migrations. Instruções, códigos de saída e decisões em [comandos-contas.md](comandos-contas.md). Build sem avisos/erros e 266 testes aprovados, incluindo execução do DLL real sobre banco isolado. Nenhuma conta real foi alterada. Próxima etapa: consulta administrativa global de scores; tema, inspeção visual e referência do protótipo continuam pendentes.

## Incremento 17 — consulta administrativa global de scores

Implementadas listagem global paginada em 20 e consulta completa de histórico, exclusivamente para Administrador. Autorização vigente no servidor nas camadas Web/Application; CRUD continua restrito ao autor. Relatórios reutilizam o corpo de apresentação da Cooperativa sem recalcular. Detalhes em [administracao-scores.md](administracao-scores.md). Build sem avisos/erros e 284 testes aprovados (129 Domain, 61 Application, 16 Infrastructure e 78 Web), sem alteração de dados reais. Permanecem tema, inspeção visual e referência do protótipo ausente.
