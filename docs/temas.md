# Tema claro e escuro

O seletor Tema está disponível na navegação pública e autenticada, com opções Sistema, Claro e Escuro. Sistema é o padrão: acompanha prefers-color-scheme e reage a mudanças do sistema enquanto a página está aberta. Escolhas explícitas prevalecem sobre o sistema.

## Decisões de apresentação

A preferência é local ao navegador/origem, compartilhada entre abas e independente da conta. Não é gravada no SQLite nem sincronizada entre dispositivos. O localStorage contém somente score-rebanho.tema, com light ou dark; selecionar Sistema remove essa chave. Valores desconhecidos retornam ao padrão Sistema.

O script tema.js aplica data-bs-theme antes das folhas de estilo, aproveitando as variáveis do Bootstrap 5.3.3 já instalado. Não há biblioteca adicional, endpoint, cookie próprio, migration ou alteração das camadas Domain/Application/Infrastructure. Se o armazenamento estiver bloqueado, a seleção funciona na página atual; após navegar, retorna ao sistema. Sem JavaScript, o layout mantém o tema claro e não exibe um seletor inoperante; os formulários continuam disponíveis.

Fundos e textos fixos foram substituídos por classes Bootstrap que acompanham o tema. Mensagens de validação usam a cor de ênfase correspondente, e botões verdes de contorno recebem as variáveis do tema escuro. As cores fixas e estilos de template sem uso foram removidos do CSS do layout. O rodapé usa fluxo flexível e não depende mais de posicionamento absoluto. A navegação colapsa abaixo do breakpoint lg.

O seletor usa label associado e select nativo, acessível por teclado. As escolhas não mudam entradas, cálculos, relatórios autorizados ou permissões.

## Verificações realizadas

- node --check src/ScoreRebanho.Web/wwwroot/js/tema.js: sintaxe válida.
- Build da solução em .local/verificacao: sem avisos ou erros.
- Suíte MSTest existente: 284 testes aprovados (129 Domain, 61 Application, 16 Infrastructure e 78 Web), incluindo formulários, relatórios e autorização.

Essas verificações não substituem a execução do seletor em navegador. Não foi declarada validação visual ou validação dinâmica de localStorage/matchMedia. Ainda é necessário conferir persistência ao navegar/reabrir, mudança automática do sistema, navegação por teclado, telas estreitas e legibilidade dos formulários/relatórios nos dois temas.

A inspeção visual geral permanece pendente; a tentativa anterior de Computer Use foi encerrada por falta de confirmação da URL do navegador. A referência docs/ideias-prototipo-v6.md continua ausente, portanto não é possível conferir a ordem e os detalhes do protótipo.

## Verificação complementar em 01/11/2026

Após a configuração externa de premissas, a solução compilou sem avisos/erros. A sintaxe de tema.js foi novamente verificada; 11 cenários de lógica de tema passaram em VM Node com APIs simuladas. Isso não valida renderização nem comportamento das APIs reais do navegador. Testes MSTest HTTP adicionais verificam que CSS e JavaScript referenciados pelo layout público/autenticado são servidos com tipo correto e conteúdo não vazio.

A nova tentativa de inspeção foi bloqueada pela indisponibilidade do host de navegador e do canal nativo de Computer Use, antes de abrir qualquer página. Evidências e roteiro para concluir a inspeção estão em [verificacao-interface.md](verificacao-interface.md). A validação visual/dinâmica real continua pendente.
