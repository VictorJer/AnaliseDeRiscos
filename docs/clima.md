# Localização e acompanhamento meteorológico

## Entrega 1 — localização opcional

A SPEC 19 registra a ampliação aprovada. Latitude e longitude são opcionais como par; limites de -90 a 90 e -180 a 180 graus são validados pelo servidor. O CRUD persiste ou remove o ponto junto da gravação existente, respeitando autoria, concorrência e novo histórico. Localização não participa das fórmulas nem altera snapshots anteriores.

A migration acrescenta duas colunas opcionais. Foi verificada a atualização de banco anterior preservando conta, rebanho e análise sem coordenadas. Nenhuma migration foi aplicada ao banco real pela verificação.

Verificação da entrega: build isolado sem avisos/erros; 131 testes Domain, 63 Application, 29 Infrastructure e 80 Web passaram na execução geral. Os dois testes restantes, de migration, passaram após corrigir a expectativa da quantidade de migrations e os dados fictícios do banco legado. As primeiras tentativas encontraram arquivos da aplicação aberta em uso e restrições de acesso ao Windows; a verificação válida usou `.local/clima` e execução autorizada com bancos isolados.

## Entrega 2 — seleção no mapa

Cadastro e edição oferecem mapa Leaflet 1.9.4, busca explícita de município no Brasil pela Open-Meteo/GeoNames e GPS solicitado somente após clique. É possível selecionar por clique/toque ou navegar pelo teclado e usar o centro do mapa. A escolha feita no mapa/GPS/busca preenche coordenadas com seis casas decimais; os valores manuais são preservados até nova seleção. A busca seleciona o ponto de referência do município, devendo ser ajustado para a propriedade.

O mapa usa os tiles HTTPS padrão do OpenStreetMap, com atribuição visível, cache normal do navegador e sem pré-carregamento de regiões. Leaflet e sua licença estão versionados localmente. A busca é feita pelo servidor com timeout de dez segundos; o endpoint exige autenticação e limita consultas por usuário. As APIs externas não recebem identificadores de contas ou nomes de rebanhos. O mapa pode revelar ao provedor a região visualizada; o GPS é processado no navegador e o ponto só é salvo ao submeter o formulário.

Sem JavaScript ou quando mapa/busca/GPS falham, o formulário continua permitindo coordenadas manuais ou ausência de localização. Falhas externas não impedem salvar o rebanho.

Verificação: sintaxe de localizacao.js válida; quatro testes Infrastructure de busca e três testes Web de mapa/localização aprovados, com HTTP simulado e SQLite isolado. A validação visual em navegador será registrada separadamente, sem equiparar os testes HTTP à inspeção visual.

Referências técnicas consultadas:

- https://leafletjs.com/reference.html
- https://operations.osmfoundation.org/policies/tiles/
- https://open-meteo.com/en/docs/geocoding-api
- https://open-meteo.com/en/terms
