# Apresentação das telas

A reformulação usa o Bootstrap já distribuído no projeto. Não adiciona framework, pacote, fonte externa ou dependência de rede. `apresentacao.css` concentra a identidade visual e complementa o CSS existente, cuja alteração local preexistente foi preservada.

## Organização

- Layout compartilhado com símbolo vetorial de pastagem, identificação da aplicação, seletor de tema no cabeçalho e navegação responsiva. Abaixo de 1200 px, os links e controles de conta usam o menu recolhível. Marca, tema e botão do menu cabem na largura de celular.
- Paleta verde com superfícies claras ou escuras, cartões, campos e botões arredondados, tabelas agrupadas e foco preservado. O seletor mantém os mesmos identificadores e comportamento de preferência.
- Página inicial com apresentação da finalidade, ações de cadastro/entrada e descrição das informações da análise, sem resultados numéricos fictícios.
- Listagem e acompanhamento com cabeçalhos destacados e ações visíveis; formulários com cabeçalho e guia estático de organização dos dados. O guia não impõe novas etapas nem depende de JavaScript.
- Relatórios com identificação, ações e score destacados. A cor da classificação usa o código já calculado pelo servidor, sem recalcular ou introduzir limites na View/CSS. As saídas por perfil permanecem separadas.
- Cartões diários de previsão, contas e administração recebem a mesma linguagem visual. Nenhum dado de negócio adicional é enviado ao navegador.

Foram preservados formulários HTTP, CSRF, mapeamentos, permissões, componentes restritos, valores e fórmulas. As regras condicionais de arrendamento e localização continuam funcionando. Não houve migration ou alteração de dados nesta reformulação.

## Verificação

Build da solução em `.local/clima` sem avisos/erros e suíte completa com **325 testes MSTest aprovados**: 131 Domain, 67 Application, 38 Infrastructure e 89 Web. Os testes Web incluem recursos estáticos, fluxos, validação, autorização e ausência de informações restritas.

A prévia real foi inspecionada no navegador integrado do VS Code, com a instância isolada da porta 5188. Foram conferidos formulário, listagem e resumo autorizado em desktop escuro; relatório e menu em 360 × 800 nos temas claro/escuro; acompanhamento em celular claro; início em desktop claro e 390 × 844; entrada em celular claro. Botões, campos, textos e cartões não apresentaram corte horizontal nas telas observadas. A conta e os registros da instância de verificação são destinados a teste; a aplicação original e seu banco foram preservados.

Essa conferência não equivale a medir formalmente todos os contrastes ou testar todos os estados de todas as páginas. GPS real e demais pendências funcionais de inspeção continuam registradas em `verificacao-interface.md`; não foram ampliadas as regras de produto.
