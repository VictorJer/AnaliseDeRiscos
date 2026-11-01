# Persistência de rebanhos e scores

## Modelo

- Rebanhos mantém identificação, autor, datas UTC, token de versão e referência ao último score.
- ScoresHistoricos mantém autor, origem opcional, nome/propriedade no momento do cálculo, data UTC, versão das premissas, score/classificação e snapshot JSON completo da análise.
- O JSON usa formato 1 e preserva valores decimal sem conversão intermediária para double. Não é usado para recalcular consultas.
- As datas são armazenadas como ticks UTC em INTEGER, permitindo ordenação cronológica no SQLite.
- O último score possui vínculo composto com seu autor, impedindo associação a um score de outra conta.
- RebanhoOrigemId é uma identificação histórica sem FK para Rebanhos: a exclusão do cadastro não apaga nem altera essa origem.
- FKs para Identity impedem exclusão em cascata das análises. Exclusão de contas não integra esta entrega.

## Gravação e leitura

Criar e atualizar incluem cadastro e novo snapshot no mesmo SaveChanges assíncrono, usando a transação automática do EF Core. Uma falha em qualquer comando desfaz a operação inteira. Conflitos de concorrência não deixam um score extra.

Cada atualização troca o GUID de versão. A versão esperada é conferida antes da gravação e o token também integra o WHERE da atualização/exclusão do EF Core. Autor é imutável e igualmente conferido na escrita. Por decisão técnica, a exclusão também exige a versão apresentada na confirmação, evitando remover um cadastro modificado desde então.

Os repositórios filtram autoria na consulta, listagem, edição e exclusão. Não há exceção para administradores nesse contrato de CRUD. A listagem tem 20 registros por página, ordenados por atualização decrescente, com desempate por identificador; páginas além do total são vazias.

Histórico avulso não cria rebanho. Consultar o cadastro ou o histórico desserializa o snapshot salvo, sem invocar calculadoras. Não foram expostas novas rotas HTTP.

Os repositórios são contratos técnicos internos. A próxima etapa de Application resolverá o usuário autenticado, validará e calculará entradas, traduzirá estados de persistência para FluentResults e entregará somente as informações autorizadas por perfil. Nenhuma identidade ou análise recebida diretamente do navegador deve ser enviada a esses contratos.

## Imutabilidade do histórico

O DbContext recusa alterar/excluir entidades históricas rastreadas. A migration acrescenta triggers SQLite que impedem UPDATE e DELETE inclusive por SQL direto. Não há operação de edição/exclusão no contrato de histórico. Futuras migrations que reconstruam a tabela devem preservar essas triggers.

A exclusão de um rebanho remove somente seu cadastro. Históricos mantêm entradas, resultados, premissas, data, autoria e identificação originais.

## Migrations

A migration PersistirRebanhosEScores foi gerada com EF Core 10.0.13 e revisada para incluir as triggers. A inicial de Identity não foi alterada. O pacote Design é privado da Infrastructure; a factory usa banco em memória e o schema Identity compatível com a Web, sem executar a inicialização da aplicação nem tocar em contas reais.

Para gerar futuras migrations, instale a ferramenta local e execute:

    dotnet tool install dotnet-ef --tool-path .local/tools --version 10.0.13
    .local/tools/dotnet-ef migrations add NomeDaMigration --project src/ScoreRebanho.Infrastructure --startup-project src/ScoreRebanho.Infrastructure --context ContasDbContext --output-dir Persistencia/Migrations

A aplicação existente aplica migrations ao iniciar. Nesta entrega a nova migration foi aplicada somente em bancos de teste isolados, incluindo a evolução de um banco com contas Identity existentes.

## Verificação

A suíte completa passou com 173 testes: 129 Domain, 19 Application, 13 Infrastructure e 12 Web. Build sem avisos ou erros, com saída em .local/verificacao para preservar a aplicação aberta.

Os testes de Infrastructure usam SQLite em memória com nome exclusivo por teste e conexões independentes. Cobrem persistência/recuperação exata do JSON, atualização sem reescrever histórico, exclusão preservando scores, autoria, paginação, nomes repetidos, versões obsoletas, conflito entre contextos, rollback em falhas simuladas, imutabilidade por SQL, cancelamento e preservação de contas na migração.

Permanecem pendentes os casos de uso CRUD com autorização na Application, relatórios por perfil, telas MVC e consulta administrativa global de scores.
