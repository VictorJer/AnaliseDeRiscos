# Referência reconstruída da interface — v6 (P1)

## Origem e limite desta referência

O protótipo original citado pela seção 15 da SPEC não foi localizado entre os arquivos do repositório nem em seu histórico Git. Este documento foi reconstruído em 01/11/2026 a partir da SPEC vigente e das Razor Views existentes. Ele resolve a referência documental ausente e descreve a implementação atual para manutenção; não é uma recuperação do original e não comprova fidelidade visual ao protótipo original.

A SPEC e o AGENTS.md continuam sendo as fontes das regras de negócio, cálculos, permissões e arquitetura. Esta referência não aprova fórmulas, novos módulos ou alterações de acesso. A ordem abaixo registra a organização existente; uma comparação com o original depende de recuperar esse material. A validação em navegador tem situação separada em [verificacao-interface.md](verificacao-interface.md).

## Navegação e apresentação comuns

- Identidade: Score do Rebanho; linguagem em português do Brasil; valores em reais, vírgula decimal e unidades explícitas.
- Layout Bootstrap responsivo, contêiner principal, navegação recolhível abaixo de lg e tabelas em contêiner com rolagem horizontal.
- Tema Sistema, Claro e Escuro; preferência local ao navegador. A navegação exibe o seletor quando JavaScript está disponível. Ver [temas.md](temas.md).
- Sem autenticação: apresentação pública, cadastro e entrada. Com autenticação: Meus rebanhos, Nova simulação, alteração de senha e saída. Administração de scores depende do papel Administrador.
- O menu não é a fronteira de segurança: Web e Application validam as permissões em cada operação.

## Home e rebanhos

A home autenticada lista somente registros do autor, em páginas de 20, ordenados pela última atualização. Mostra nome, propriedade, data, último score/classificação e ações Consultar, Editar e Excluir. O estado vazio explica como iniciar e apresenta Cadastrar meu primeiro rebanho.

O formulário compartilhado de cadastro/edição segue esta ordem:

1. Identificação: nome e propriedade textual.
2. Terreno e pastagem: posse, pastagem, área, arrendamento, piquetes, pasto pronto e rotação.
3. Lote: quantidade, peso médio, preço de compra, dias e raça pura.
4. Saúde e manejo: vacina de carbúnculo, controle de carrapato, vermífugo e modificador orgânico.
5. Conferência de ocupação, após solicitação ao servidor.
6. Ações Conferir ocupação, Salvar e gerar score e Cancelar; aviso de estimativa.

Nome/propriedade pertencem ao cadastro de rebanho, sem cadastro separado de propriedades. A simulação avulsa reutiliza as etapas de terreno, lote e manejo e oferece Conferir ocupação, Gerar score, Novo score e Meus rebanhos.

As etapas são grupos do mesmo formulário, sem exigir um assistente JavaScript. Sem JavaScript, as operações de formulário continuam por POST. Entradas inválidas preservam valores, exibem resumo e erros junto dos campos. A conferência mostra somente ocupação, capacidade, quantidade compatível e versão; não apresenta score/financeiro nem grava histórico.

Salvar ou gerar score valida e recalcula no servidor. Consultar uma análise já salva não recalcula. Edição preserva os dados anteriores; conflitos não sobrescrevem outra sessão. Exclusão tem tela de confirmação e POST com CSRF, preservando históricos.

## Relatório do Pecuarista

A ordem implementada é:

1. Identificação do rebanho/propriedade, quando houver, e ações de navegação, edição e nova análise.
2. Data da análise, versão das premissas, score total inteiro, classificação e explicação resumida.
3. Alertas prioritários e recomendações.
4. Pasto e animais: ocupação, capacidade em UA, animais compatíveis e peso estimado de venda por animal.
5. Resumo financeiro: custo total, capital inicial, receita, lucro/prejuízo e margem sobre custo.
6. Preço de equilíbrio por kg vivo com explicação prática.
7. Resumo do lucro/prejuízo no cenário de queda de 10% no preço de venda.
8. Avisos sobre estimativas, despesas excluídas do capital e ausência de garantia de lucro/crédito.

Os detalhes exclusivos da Cooperativa não são enviados no HTML, JSON ou atributos. As entradas podem ser acessadas no formulário de edição autorizado. Alertas relevantes, incluindo prejuízo, superlotação e sanidade, continuam visíveis.

## Relatório da Cooperativa e consulta administrativa

O relatório completo próprio usa a mesma análise e segue a ordem implementada:

1. Cabeçalho, identificação, data, score, classificação e explicação.
2. Alertas prioritários e recomendações.
3. Pontuação e justificativa dos quatro componentes do score.
4. Capacidade e produção: UA, ocupação, rotação, comparação com/sem rotação, GMD, mortalidade, animais vendidos, peso e arrobas.
5. Composição dos custos da operação e do capital inicial, distinguindo investimento integral e parcela econômica.
6. Produtos, doses, aplicações/dias, consumo, embalagens e custo.
7. Indicadores financeiros de referência e do cenário adverso; preço de equilíbrio.
8. Dados informados consolidados.
9. Versão e valores das premissas.
10. Fórmulas e memória textual de cálculo preservadas no snapshot.
11. Limites e avisos da estimativa.

A administração reutiliza a apresentação completa para consultas autorizadas, com listagem global em páginas de 20 e sem edição/exclusão de scores. Cooperativa, por si só, continua acessando apenas registros próprios. O papel Administrador é independente do perfil e não amplia o CRUD de rebanhos.

## Prioridade dos alertas

A implementação existente ordena: prejuízo, superlotação, ausência de vacina/controle de carrapato/vermífugo, margem baixa, prejuízo no cenário adverso, subocupação e rotação com piquetes insuficientes. A ordem está localizada no avaliador de domínio e coberta por testes; as Views a preservam. Esta documentação registra a decisão já adotada, sem alterar regras ou fazer recomendações independentes dos resultados.

## Histórico

A análise histórica informa que preserva o cálculo original. Entradas, resultados, fórmulas quando disponíveis e premissas vêm do snapshot, sem substituição pelos parâmetros atuais. O perfil vigente determina a representação entregue. Históricos antigos sem fórmulas textuais exibem essa limitação, sem inventar memória retroativa.

## Evoluções que exigem definição própria

A documentação original mencionava localização, propriedades, comparação/exclusão, clima, mercado e IA como temas em aberto. O escopo posterior resolveu propriedade textual e exclusão de cadastro na seção 16, preservando os scores históricos. Permanecem fora desta entrega:

- Localização geográfica e cadastro separado de propriedades/produtores.
- Comparação de versões de análises e exclusão de scores históricos.
- Integrações de clima e cotações de mercado.
- Explicações ou decisões geradas por IA.
- Vínculo de cooperativa/cooperados, compartilhamento e gestão com vários usuários.

Não há autorização implícita para implementar esses itens. A próxima validação visual deve seguir o roteiro de verificacao-interface.md. A eventual recuperação do protótipo original exige comparar e registrar divergências antes de alterar esta referência.
