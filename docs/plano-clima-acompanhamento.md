# Plano de implementação — localização, previsão e acompanhamento

## Situação

Plano elaborado com as escolhas do usuário em 01/11/2026. As quatro etapas de implementação foram concluídas: localização opcional, mapa/busca/GPS, consulta com cache e telas de acompanhamento. Build e 325 testes MSTest passaram. A inspeção posterior no navegador integrado do VS Code conferiu as novas telas em desktop/celular, nos temas claro/escuro, e identificou um ajuste de rolagem do mapa que foi corrigido e verificado. GPS em aparelho real, falhas de rede no navegador e execução visual sem JavaScript continuam pendentes. Evidências, decisões técnicas adotadas e limites da verificação estão em [clima.md](clima.md) e [verificacao-interface.md](verificacao-interface.md). Fórmulas e premissas aprovadas permanecem vigentes.

## Decisões aprovadas

- Implementar as etapas de localização, previsão de sete dias e acompanhamento.
- Localização opcional por rebanho, tanto no cadastro quanto na edição. Registros existentes continuam válidos sem localização.
- Mapa para selecionar um ponto, com busca de município e GPS opcional. Persistir latitude e longitude; não desenhar polígonos ou calcular área pelo mapa nesta entrega.
- Sem localização, apresentar “Adicionar localização para ver a previsão”. A ausência não impede gerar score.
- Open-Meteo como fonte meteorológica para demonstração.
- Previsão de sete dias com temperatura, chuva e vento.
- Tela Acompanhamento com cartões dos rebanhos do usuário e botão Ver previsão para abrir os detalhes.
- Consultar ao abrir os detalhes e reutilizar o resultado por até 30 minutos, informando o horário da consulta. Não é atualização automática em segundo plano a cada 30 minutos.
- Em falha da consulta, informar indisponibilidade.
- Alertas climáticos, recomendações automáticas e efeitos do clima no score ficam para outra entrega. Mercado, IA, diagnóstico geográfico e redesenho geral do sistema também ficam fora deste plano.

## Etapa 1 — contratos, localização e persistência

1. Registrar a ampliação na SPEC antes de implementar código: a autorização do usuário amplia o escopo anterior, que deixava localização e integrações externas para decisão posterior.
2. Representar localização como valor opcional com latitude e longitude válidas. Aceitar ambas ausentes; recusar pares incompletos e valores fora dos limites geográficos. Não inferir a localização pela identificação textual da propriedade.
3. Acrescentar localização aos contratos apropriados da Application e aos ViewModels Web separados, com mapeamentos AutoMapper explícitos.
4. Criar migration EF Core com colunas opcionais no rebanho, sem preencher coordenadas fictícias em registros antigos. Não modificar snapshots históricos.
5. Preservar autoria, concorrência, CSRF e gravação atômica existentes. Cadastro e edição do rebanho continuam seguindo as regras vigentes de geração de histórico.

Aceite: é possível salvar, consultar, alterar e remover a localização de um rebanho próprio; dados inválidos são recusados no servidor; sem localização o fluxo atual funciona; terceiros não conseguem consultar ou alterar a localização por identificador direto.

## Etapa 2 — mapa e seleção do ponto

1. Inserir seleção de localização no formulário compartilhado de cadastro/edição, mantendo Bootstrap.
2. Oferecer busca de município com seleção explícita quando houver mais de um resultado, mapa com marcador e seleção do ponto por clique/toque.
3. Oferecer Usar minha localização somente após ação do usuário, tratando recusa, indisponibilidade e ausência de suporte. Não solicitar GPS automaticamente.
4. Mostrar o ponto selecionado e permitir remover a localização. Buscar município deve ajudar a posicionar o mapa; a tela deve deixar claro qual ponto será salvo.
5. Tratar falhas do mapa e da busca sem impedir o cadastro do rebanho. Manter alternativa acessível de informar coordenadas e a possibilidade de seguir sem localização, inclusive sem JavaScript.

Escolhas técnicas para esta etapa: selecionar biblioteca de mapa, provedor de mapas e serviço de busca, conferindo documentação oficial, condições de demonstração, atribuição e limites antes de integrar. Preferir recurso simples, sem novo cadastro de propriedades ou pacote desnecessário.

Aceite: seleção funciona em celular e desktop; marcador e dados salvos coincidem; edição recupera o ponto; limpar remove ambos os valores; falhas de GPS/mapa/busca têm mensagens claras; labels e navegação por teclado permanecem utilizáveis.

## Etapa 3 — previsão meteorológica

1. Definir DTOs e contrato pequeno de consulta meteorológica na Application; implementar HTTP e interpretação da resposta na Infrastructure. Domain permanece independente de HTTP e do fornecedor.
2. Consultar sete dias para o ponto salvo, com máximas/mínimas de temperatura, precipitação e vento em unidades explícitas. Confirmar na documentação oficial a variável de vento escolhida, distinguindo velocidade de rajadas.
3. Resolver autorização e localização do rebanho no servidor antes de consultar ou devolver qualquer previsão. Não aceitar uma localização enviada pelo cliente como substituta da localização salva nesse fluxo.
4. Usar acesso assíncrono, CancellationToken, prazo de consulta limitado e tratamento de falhas esperadas com FluentResults. Não exibir detalhes técnicos ao usuário.
5. Reutilizar consultas bem-sucedidas por até 30 minutos, considerando coordenadas e parâmetros relevantes na chave de cache. Alterar o ponto deve resultar em consulta da nova localização. Autorizar toda requisição mesmo quando houver cache.
6. Mostrar fonte e momento da consulta, distinguindo-os do horário de geração do modelo meteorológico. Cache vencido exige nova consulta quando os detalhes forem abertos; nenhuma rotina periódica é necessária.
7. Não substituir falhas por dados fictícios nem tratar valores meteorológicos ausentes como zero. Proposta técnica inicial: cache em memória, com perda aceitável ao reiniciar a demonstração e sem novas tabelas de previsão.

Aceite: resposta válida cobre as datas e unidades esperadas; consultas dentro do prazo reutilizam os dados; expiração e troca de localização renovam a consulta; falhas, respostas incompletas e cancelamento são tratados; o cálculo de score não depende da disponibilidade meteorológica.

## Etapa 4 — acompanhamento e detalhes

1. Acrescentar Acompanhamento à navegação autenticada.
2. Listar em cartões somente rebanhos próprios, com nome, propriedade, estado da localização e ação apropriada. Proposta: reutilizar paginação de 20 registros do CRUD.
3. Para rebanho com ponto salvo, oferecer Ver previsão e abrir página de detalhes com sete dias, fonte, horário da consulta e navegação de retorno.
4. Para rebanho sem ponto, oferecer Adicionar localização para ver a previsão, direcionando à edição autorizada.
5. Não disparar uma consulta meteorológica por cartão ao abrir a listagem. A consulta acontece ao abrir os detalhes, como aprovado.
6. Preservar os mesmos direitos de autoria para Pecuarista, Cooperativa e Administrador. Não acrescentar dados financeiros restritos aos cartões.

Aceite: usuário encontra seus rebanhos e abre os detalhes; lista vazia orienta o cadastro; sem localização há orientação; acesso direto a registros de terceiros é recusado; previsão indisponível oferece mensagem e nova tentativa sem alterar o score.

## Etapa 5 — verificação e entrega

- MSTest de Domain/Application: coordenadas, limites, ausência, autorização e falhas esperadas.
- Testes de Infrastructure com HTTP simulado: URL/parâmetros, unidades, resposta parcial/inválida, timeout e indisponibilidade. Testar cache e expiração com tempo controlado, sem esperas reais de 30 minutos.
- Integração Web com SQLite isolado: migration e registros antigos, autoria, acesso direto, CSRF, localização opcional e fluxo de acompanhamento. Não depender de API externa para aprovar os testes automatizados.
- Verificar mapeamentos e preservar testes de cálculo e relatórios por perfil.
- Executar build e dotnet test na solução; usar saída isolada caso a aplicação aberta bloqueie arquivos, conforme histórico do projeto. Relatar os resultados efetivamente obtidos.
- Validar no navegador, em celular/desktop e temas claro/escuro: mapa, marcador, busca, GPS recusado, teclado, sete dias e estados de erro. Fazer consulta real controlada com dados fictícios e registrar o resultado separadamente dos testes simulados.
- Atualizar documentação de operação, fonte/atribuição, configuração, cache e limites da demonstração. Não usar contas ou bancos reais em testes.

## Referências e decisões técnicas ainda abertas

- SPEC, AGENTS.md e docs/comparacao-prototipo-v6.md.
- Documentação da previsão: https://open-meteo.com/en/docs
- Condições de uso consultadas para a escolha de demonstração: https://open-meteo.com/en/terms
- Na implementação, confirmar provedor de mapas/geocodificação, variável de vento, comportamento para resposta parcial e configuração de cache. São decisões localizadas, a documentar sem ampliar o escopo aprovado.
- Alertas, alterações nas fórmulas, uso comercial e módulos adicionais exigem planejamento próprio. Os limites climáticos e agronômicos do protótipo não são premissas aprovadas para o motor atual.
