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

