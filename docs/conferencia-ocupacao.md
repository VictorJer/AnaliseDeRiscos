# Conferência da ocupação

ConferirOcupacao autentica a conta e reutiliza a validação de DadosSimulacao e as premissas de lotação da mesma instância de CalcularScoreRebanho. Não calcula score, produção ou resultado financeiro, não consulta relógio e não grava cadastro nem histórico. A saída contém somente versão das premissas e indicadores de lotação.

A conferência complementa o formulário: salvar continua validando e recalculando todas as entradas no servidor. Não há confiança em resultados enviados pelo navegador.

Verificação: três testes MSTest cobrem conferência sem score/data, premissas personalizadas e recusa de conta/entrada inválida. A suíte Application passou com 48 testes.
