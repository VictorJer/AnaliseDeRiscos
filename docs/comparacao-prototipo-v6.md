# Comparação com o protótipo HTML v6

## Fonte e método

Comparação realizada em 01/11/2026 com o arquivo original fornecido pelo usuário: `C:\Users\victo\Downloads\risco-do-rebanho-dev-v6 (1).html` (112.516 bytes; SHA-256 `42E768C5137339AE9909CCC3698BA47FED07A14F10AD031AF72F0789148EA08D`). O arquivo foi somente lido e permanece fora do repositório.

Foram comparados HTML, CSS e JavaScript do protótipo com Razor Views, CSS, casos de uso e calculadoras atuais, à luz da SPEC vigente. Esta é uma comparação de código e estrutura: não houve renderização em navegador, teste das APIs externas do protótipo nem validação visual de contraste, responsividade ou interação. Build e testes não foram executados nesta etapa documental.

O protótipo original agora está disponível para consulta local. A referência em ideias-prototipo-v6.md foi reconstruída anteriormente a partir da aplicação, portanto não constitui evidência independente de fidelidade ao HTML original. A nota de ausência do original na SPEC 15 descreve a situação anterior a esta entrega do usuário.

## Resultado

A aplicação atende ao núcleo funcional aprovado, mas sua apresentação e abrangência diferem do protótipo. O original contém recursos adicionais e também regras de cálculo incompatíveis com convenções posteriormente aprovadas. Reproduzi-lo integralmente exigiria decisões de produto; não se deve copiar seu motor JavaScript para o sistema MVC.

## Interface e fluxo

| Aspecto | Protótipo original | Aplicação atual | Diferença |
| --- | --- | --- | --- |
| Marca | Risco do Rebanho | Score do Rebanho | Nome distinto; o atual segue a SPEC. |
| Identidade visual | Fonte Figtree, paleta rural, apresentação de destaque, sombras, cartões arredondados e largura final de 920 px | Tipografia e contêiner Bootstrap, barra de navegação e cartões convencionais | A identidade visual específica do original não foi reproduzida. |
| Orientação do formulário | Quatro marcadores clicáveis: Local, Terreno, Rebanho e Saúde; progresso acompanha a rolagem | Grupos numerados de Terreno, Lote e Saúde, com identificação no CRUD | Não há indicador de progresso nem etapa geográfica. O original também mantém os grupos na mesma página, não é um assistente com telas sucessivas. |
| Escolhas | Botões segmentados Sim/Não, Próprio/Arrendado e Verão/Inverno | Selects e checkboxes Bootstrap | Dados centrais equivalentes, interação distinta. O protótipo exige escolha explícita para todos os Sim/Não; checkboxes atuais representam falso quando desmarcados. |
| Campos condicionais | Oculta arrendamento e piquetes até a escolha correspondente | Campos ficam apresentados com instruções de preenchimento | Falta a apresentação condicional do original. |
| Ajuda contextual | Exemplos, explicações de manejo e custo por animal durante preenchimento | Labels, mensagens de validação e instruções mais curtas | Há menos explicação junto aos campos; a prévia de custo por animal não está presente. |
| Conferência | Botão Calcular risco, sem conferência separada de ocupação | Conferir ocupação e Gerar score, com cálculo no servidor | A aplicação possui a conferência exigida pela SPEC 15. |
| Resultado principal | Medidor semicircular SVG de risco, etiqueta de classificação e cartões de risco/lucro/ocupação | Score numérico e classificação em cartão; indicadores em listas e tabelas | Faltam o medidor e a composição visual do original. |
| Componentes | Barras visuais de fatores de risco | Tabela de pontos e justificativas no relatório completo | Apresentação distinta; os componentes devem continuar restritos conforme SPEC 17. |
| Hierarquia do relatório | Diagnóstico → indicadores rápidos → resumo narrativo → fatores → clima/local → plano de ação → produção → dinheiro → pasto → compras → salvar | Score → alertas; Pecuarista segue para resumo, Cooperativa para componentes e detalhes | Ordem e agrupamento diferentes. A separação por perfil atual não existe no original. |
| Financeiro | Cores e sinais para saídas/entradas, participação percentual de custos, lucro e gasto por animal, custo por kg de ganho | Resumo financeiro e, para Cooperativa, composição tabular de custo/capital e cenário adverso | Alguns indicadores por animal/kg e percentuais de composição não são apresentados atualmente. Novos indicadores precisam de definição e cálculo no servidor. |
| Rebanhos | Salvar após calcular; acompanhamento em cartões com clima e referências de mercado | CRUD com nome/propriedade antes de salvar, listagem paginada, consulta, edição e confirmação de exclusão | Fluxos distintos; simulação avulsa atual não tem ação para convertê-la em cadastro. |
| Temas | CSS acompanha o sistema e contém estilos para `data-theme`; não foi localizado seletor explícito no HTML/JavaScript | Sistema/Claro/Escuro por Bootstrap e preferência local | A aplicação acrescenta escolha explícita persistida; paleta e estilos distintos. |

Evidências principais: HTML original linhas 11–213 (estilos), 227–318 (telas), 443–460 (relatório) e função `rrUx` (progresso); Views `Shared/EditorTemplates/SimulacaoViewModel.cshtml`, `Simulacoes/Nova.cshtml`, `Rebanhos/Index.cshtml`, `Relatorios/Pecuarista.cshtml`, `Relatorios/_AnaliseCompleta.cshtml` e `Shared/_Layout.cshtml`; `wwwroot/css/site.css`.

## Recursos presentes no protótipo e ausentes no sistema

- Geolocalização, busca de município, mapa/satélite e desenho do contorno para calcular área.
- Diagnóstico geográfico de relevo, água, vegetação, banhado, cercas e área urbana, com disponibilidade das fontes e avisos de precisão.
- Bloqueio de cálculo para certas áreas urbanas ou fora da região considerada pelo protótipo.
- Consulta de clima histórico, previsão de sete dias e alertas de chuva, frio, geada, calor, vento e seca por rebanho.
- Mercado da semana, preços atual/anterior, tendências e sugestões de insumos associadas a alertas. O original identifica seus valores de mercado como exemplos; a existência da tela não comprova integração com cotações reais.
- Análise textual por IA, dependente de ambiente que ofereça `window.claude`.
- Painel para editar premissas, gerir usuários e papéis, editar mercado e copiar/aplicar backup de parâmetros.

Esses módulos não são pendências automáticas do escopo vigente. Mapa, clima, mercado, IA e novas permissões administrativas exigem definição própria. Hoje as premissas são configuradas externamente e versionadas; a administração Web consulta scores, e comandos locais atribuem perfis/papel. O backup do protótipo cobre valores de referência, não equivale a um backup do banco SQLite.

## Divergências de negócio que impedem copiar os resultados

| Regra | Protótipo | Sistema / SPEC vigente | Consequência |
| --- | --- | --- | --- |
| Número principal | Exibe `risco = 100 - score`; maior é pior | Exibe score; 80 ou mais indica risco baixo | Um score 85 corresponde a risco 15 no mostrador original. A inversão visual não deve mudar a fórmula ou classificação silenciosamente. |
| Clima | Reduz GMD em 3% e acrescenta 1 ponto percentual à mortalidade; pode somar perda geográfica | Não aplica esses acréscimos; usa premissas aprovadas da produção | Mesmas entradas podem produzir pesos, receitas, margens e scores diferentes. |
| Formação de verão | Custo integral / 5 × dias / 365 | Custo integral / 5, conforme SPEC 13 | Diferença de custo para períodos distintos de 365 dias. |
| Aplicações sanitárias | `max(1, teto(dias/intervalo))` | `1 + piso(dias/intervalo)`, incluindo dia zero e último dia coincidente | Em 90 dias de controle de carrapato: 1 aplicação no original e 2 no sistema; pode mudar o número de frascos. |
| Rotação com 1–2 piquetes | Não considera investimento nem benefício, pois ambos dependem de `rotOn` | Gera investimento quando marcada; benefício somente a partir de 3 piquetes | Custos e capital divergem por convenção aprovada. |
| Arrendamento zero | Rejeitado no formulário | Permitido para terreno arrendado | Validações diferem; preservar a decisão vigente. |
| Mensagem de risco baixo | Usa a expressão “criação segura” e referência a levar ao banco | Trata o score como estimativa, sem garantia de lucro, segurança ou crédito | O texto original precisa de adaptação às restrições da SPEC. |

Evidências: função `calc` do original, especialmente linhas 368–416, e validação de `gerar`; `Domain/Producao/CalculadoraProducao.cs`, `Domain/Custos/CalculadoraCustos.cs` e SPEC 13. Lotação, pesos centrais dos quatro componentes, referência inicial de R$ 340 por arroba viva de 30 kg e cenário de queda de 10% têm bases compartilhadas, mas não anulam as diferenças acima.

## Diferenças deliberadas de arquitetura e autorização

O original executa cálculo e contas no navegador, com persistência local. A aplicação usa cálculo autoritativo no servidor, Identity, EF Core/SQLite, snapshots históricos, autorização por autoria/perfil, CSRF e concorrência. Essas diferenças cumprem a arquitetura aprovada e devem ser preservadas em uma aproximação visual.

O relatório único do original expõe produção e custos detalhados. A SPEC 17 restringe parte desses dados ao relatório completo; aproximar a aparência não autoriza entregar detalhes da Cooperativa ao Pecuarista. A administração atual acrescenta consulta histórica global paginada, funcionalidade distinta do painel de parâmetros do HTML.

## Ordem sugerida para aproximação

1. Aproximar apresentação, espaçamento, cores, cartões e ajuda contextual com componentes Bootstrap, mantendo o nome e as regras vigentes.
2. Acrescentar navegação entre grupos e campos condicionais como melhoria progressiva, preservando o fluxo sem JavaScript.
3. Aproximar a composição visual dos relatórios com indicadores e medidor de score claramente identificado; barras de componentes apenas para perfis autorizados. Qualquer mudança para índice invertido de risco exige decisão explícita.
4. Validar as telas em navegador, em celular/desktop e em ambos os temas, usando banco isolado.
5. Definir separadamente o escopo de mapa, clima, mercado, IA e administração adicional antes de implementá-los.

Nenhum cálculo, permissão, tela ou dado real foi alterado nesta comparação. A alteração local de CSS já existente foi preservada.
