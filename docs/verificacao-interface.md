# Verificação da interface

## Reformulação da apresentação com Bootstrap — 01/11/2026

Atualizados layout compartilhado, marca, cores, cartões, campos, tabelas e hierarquia de início, formulários, relatórios e acompanhamento. Build sem avisos/erros e 325 testes MSTest aprovados novamente. A inspeção no navegador integrado do VS Code cobriu desktop, 360 × 800 e 390 × 844, com telas em claro/escuro e navegação responsiva. Escopo, telas efetivamente observadas e limites em [apresentacao.md](apresentacao.md). Nenhuma fórmula, permissão ou dado real foi alterado.

## Validação posterior no navegador integrado do VS Code — 01/11/2026

A inspeção das novas telas foi realizada pelo navegador integrado do VS Code, acessado como aplicativo nativo. Essa rota funcionou apesar da indisponibilidade do host de automação de navegador independente. Foi usada exclusivamente a instância `http://127.0.0.1:5188`, com banco separado em `.local/clima-vscode-preview/validacao.db`, conta fictícia e ponto público de Lages. A instância antiga na porta 5288 não foi modificada.

Verificações observadas em tela:

- Acompanhamento com cartão, identificação do rebanho e ação Ver previsão.
- Sete dias reais de previsão em desktop (1366 × 768, além de 1366 × 763 na primeira inspeção), e larguras de 390 × 844 e 360 × 800. Cartões, unidades, fonte, avisos e ações permaneceram legíveis, sem corte horizontal observado.
- Temas claro/escuro, troca no seletor, manutenção da preferência entre páginas e abertura/fechamento do menu em tela estreita.
- Formulário de edição e mapa real em desktop escuro e celular claro; seleção por clique preencheu o par de coordenadas com seis casas decimais.
- Busca real de Lages, seleção explícita de Santa Catarina entre resultados de estados diferentes e posicionamento do ponto de referência.
- Remover localização retirou o marcador e esvaziou latitude/longitude; Usar centro do mapa voltou a preencher o par; Tab avançou ao campo de latitude com foco visível.

Foi observado que a roda do mouse sobre o mapa alterava o zoom ao tentar percorrer o formulário. A configuração foi ajustada para `scrollWheelZoom: false`, preservando botões de zoom. Após recompilar, reiniciar somente a instância isolada e recarregar, rolar sobre o mapa passou a percorrer a página.

Verificação pelo terminal: build sem avisos/erros, sintaxe JavaScript válida e 325 testes MSTest aprovados novamente (131 Domain, 67 Application, 38 Infrastructure e 89 Web). Após o ajuste de rolagem, build e quatro testes Web de mapa/layout passaram. O fluxo HTTP real pela aplicação também aprovou cadastro fictício, busca de município, previsão de sete dias, acesso de terceiro recusado, par incompleto recusado, remoção do ponto e preservação da versão do rebanho durante a consulta da previsão.

Limites: não foi solicitada a localização real do aparelho. Permissão/recusa e precisão do GPS em dispositivo real continuam pendentes. O comportamento sem JavaScript foi coberto pelo fluxo HTTP, sem desativar JavaScript no navegador desta inspeção. Sincronização entre abas, falhas de rede em navegador e o roteiro geral de todas as telas antigas continuam com suas situações próprias; esta validação não os declara concluídos.

## Incremento de localização e clima — 01/11/2026

Implementados mapa opcional, busca de município/GPS, previsão de sete dias e acompanhamento. Build sem avisos/erros e 325 testes MSTest aprovados; detalhes em [clima.md](clima.md). A consulta real da previsão foi conferida com o ponto público do município de Lages.

Uma instância separada com banco exclusivo foi iniciada para inspeção e encerrada após o bloqueio. A descoberta informou host de navegador indisponível; a tentativa pelo navegador nativo foi encerrada pelo Computer Use porque não foi possível determinar a URL atual no Windows com segurança. Não houve interação posterior nem validação visual das novas telas. O roteiro abaixo continua pendente e deve incluir seleção do ponto, busca, GPS recusado, remoção da localização e acompanhamento nos dois temas.

## Situação em 01/11/2026

A inspeção visual/dinâmica em navegador permanece bloqueada. A descoberta de superfícies retornou `The browser access host is unavailable`; o runtime nativo de Windows retornou `Computer Use native pipe is unavailable` com erro 2 (arquivo/canal não encontrado). Nenhuma página foi capturada ou operada nessa tentativa. Não houve rejeição de autorização nem falta de confirmação de URL nesta execução: o bloqueio atual é a indisponibilidade dos serviços de automação.

Não foi iniciado servidor ou alterado banco real para essa tentativa. Os testes HTTP usam SQLite isolado.

## Evidências obtidas sem navegador

- Build isolado da solução: sem avisos ou erros.
- Suíte anterior a esta etapa: 297 testes MSTest aprovados, incluindo fluxos HTTP, decimais pt-BR, validação, CSRF, autoria, concorrência e relatórios por perfil.
- Verificação de sintaxe com `node --check src/ScoreRebanho.Web/wwwroot/js/tema.js`: aprovada.
- Diagnóstico do JavaScript original em VM Node com document, matchMedia e localStorage simulados: 11 cenários aprovados. Cobriu tema inicial claro/escuro antes do DOM, preferência explícita, valor inválido, persistência ao recriar a página simulada, retorno ao sistema, mudanças do sistema com/sem preferência explícita, sincronização e limpeza entre abas e armazenamento bloqueado.
- O diagnóstico em VM não usa navegador e não valida renderização, CSS, foco, contraste ou APIs reais do navegador. É uma verificação complementar, fora da suíte MSTest.
- Adicionados testes MSTest HTTP que extraem os recursos CSS/JavaScript do HTML público e autenticado e verificam status, tipo de conteúdo e corpo não vazio. Isso inclui o script do tema e o Bootstrap usado pela navegação responsiva.

## Roteiro para concluir no navegador

Usar uma instância separada e banco SQLite exclusivo para teste, sem contas ou rebanhos reais. Criar os dados fictícios pelos mecanismos de teste existentes. Não validar contra a aplicação aberta de produção/desenvolvimento com dados reais.

| Verificação | Procedimento e resultado esperado | Situação |
| --- | --- | --- |
| Tela estreita | 360 × 800 e 390 × 844; formulários e botões legíveis, sem conteúdo cortado; tabelas rolam dentro de seu contêiner | Pendente |
| Desktop | 1366 × 768; conferir home, cadastro/edição, simulação e ambos os relatórios | Pendente |
| Navegação | Abrir/fechar menu no celular; acessar links e ações por teclado | Pendente |
| Foco | Percorrer campos, seletores, checkboxes e botões com Tab/Shift+Tab; foco sempre visível | Pendente |
| Temas | Inspecionar claro e escuro em todas as telas acima, incluindo erros, alertas, tabelas e botões | Pendente |
| Persistência real | Selecionar tema, navegar, recarregar e reabrir; a escolha explícita deve permanecer | Pendente |
| Tema do sistema | Selecionar Sistema e emular claro/escuro; acompanhar alterações sem recarregar | Pendente |
| Abas e armazenamento | Alterar preferência em outra aba, limpar armazenamento e bloquear armazenamento; conferir os fallbacks documentados | Pendente |
| Sem JavaScript | Validar, conferir ocupação, gerar score e editar por POST normal; seletor de tema permanece oculto | Pendente |
| Formulário | Erros junto dos campos, valores preservados, vírgula decimal e conferência sem score/financeiro | Pendente visual; HTTP automatizado aprovado |
| Relatórios | Conferir identificação, valores/unidades, alertas e permissões conforme SPEC 17 | Pendente visual; HTTP automatizado aprovado |

Não encerrar esta etapa como visualmente aprovada até registrar as telas e os resultados reais das verificações acima. A referência de interface em ideias-prototipo-v6.md deve distinguir documentação reconstruída de um protótipo original recuperado.

## Correção identificada na verificação HTTP

Os dois testes novos inicialmente falharam: a requisição a /ScoreRebanho.Web.styles.css retornou HTTP 500, com FileNotFoundException no handler de assets. Esse arquivo era referenciado pelo layout e a folha isolada continha somente uma regra para a marca da navegação.

A regra white-space: normal foi movida para site.css, a folha isolada foi removida e o layout deixou de solicitar o bundle gerado. O comportamento de quebra de linha da marca foi preservado com uma regra específica. Os testes percorrem os recursos efetivamente referenciados no HTML e cobrem a regressão nas páginas pública e autenticada. Isso corrige a falha HTTP observada, sem declarar validação visual.

Resultado após a correção: build isolado sem avisos/erros e 80 testes Web aprovados. Somados às demais camadas já verificadas nesta execução, 299 testes MSTest aprovados. A inspeção visual permanece pendente.
