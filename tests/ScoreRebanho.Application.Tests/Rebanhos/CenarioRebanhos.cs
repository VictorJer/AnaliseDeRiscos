using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Domain.Pastagens;
using ScoreRebanho.Domain.Rebanhos;
using ScoreRebanho.Domain.Simulacoes;

namespace ScoreRebanho.Application.Tests.Rebanhos;

internal sealed class CenarioRebanhos
{
    internal UsuarioTeste Usuario { get; } = new();
    internal PersistenciaTeste Persistencia { get; } = new();
    internal RelogioTeste Relogio { get; } = new();
    internal CalcularScoreRebanho Calcular { get; }
    internal CriarRebanho Criar { get; }
    internal AtualizarRebanho Atualizar { get; }
    internal ConsultarRebanho Consultar { get; }
    internal ConsultarEdicaoRebanho Edicao { get; }
    internal ListarRebanhos Listar { get; }
    internal ExcluirRebanho Excluir { get; }
    internal ConsultarHistoricoProprio Historico { get; }

    internal static CriarRebanhoDto Entrada => new("Lote", "Fazenda", new()
    {
        AreaHectares = 100, Pastagem = TipoPastagem.Verao, Posse = TipoPosse.Proprio,
        PastoPronto = true, QuantidadeAnimais = 100, PesoCompraKg = 450, PrecoCompraPorKgReais = 10,
        DiasPastagem = 180, VacinaCarbunculo = true, ControleCarrapato = true, Vermifugo = true
    });

    internal CenarioRebanhos()
    {
        Calcular = new(Relogio);
        Criar = new(Usuario, Persistencia, Calcular);
        Atualizar = new(Usuario, Persistencia, Calcular);
        Consultar = new(Usuario, Persistencia);
        Edicao = new(Usuario, Persistencia);
        Listar = new(Usuario, Persistencia);
        Excluir = new(Usuario, Persistencia);
        Historico = new(Usuario, Persistencia);
    }

    internal sealed class UsuarioTeste : IUsuarioAtual
    {
        internal ContaAutenticada? Conta { get; set; } = new("autor-a", PerfisNegocio.Pecuarista);
        public ValueTask<ContaAutenticada?> ObterAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Conta);
    }

    internal sealed class RelogioTeste : TimeProvider
    {
        internal int Chamadas { get; private set; }
        public override DateTimeOffset GetUtcNow()
        {
            Chamadas++;
            return DateTimeOffset.UnixEpoch;
        }
    }

    internal sealed class PersistenciaTeste : IRepositorioRebanhos, IRepositorioScores
    {
        private readonly Dictionary<(Guid Id, string Autor), RebanhoPersistidoDto> registros = [];
        private readonly Dictionary<(Guid Id, string Autor), ScoreHistoricoDto> historicos = [];
        internal int Chamadas { get; private set; }
        internal int Gravacoes { get; private set; }
        internal string? UltimoAutor { get; private set; }
        internal CancellationToken UltimoToken { get; private set; }
        internal ResultadoGravacaoRebanho? RespostaAtualizacao { get; set; }
        internal EstadoGravacaoRebanho? RespostaExclusao { get; set; }
        internal bool FalharCriacao { get; set; }

        private void Chamada(string autor, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Chamadas++;
            UltimoAutor = autor;
            UltimoToken = token;
        }

        public Task<RebanhoGravadoDto> CriarAsync(string autorId, IdentificacaoRebanho identificacao, AnaliseCompletaDto analise, CancellationToken cancellationToken = default)
        {
            Chamada(autorId, cancellationToken);
            if (FalharCriacao)
                throw new InvalidOperationException("Defeito simulado.");
            var salvo = new RebanhoGravadoDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            registros[(salvo.Id, autorId)] = new(salvo.Id, identificacao.Nome, identificacao.Propriedade,
                salvo.Versao, salvo.UltimoScoreId, analise.CalculadaEmUtc, analise.CalculadaEmUtc, analise);
            historicos[(salvo.UltimoScoreId, autorId)] = new(salvo.UltimoScoreId, autorId, salvo.Id, identificacao.Nome,
                identificacao.Propriedade, analise.CalculadaEmUtc, analise);
            Gravacoes++;
            return Task.FromResult(salvo);
        }

        public Task<ResultadoGravacaoRebanho> AtualizarAsync(Guid id, string autorId, Guid versaoEsperada, IdentificacaoRebanho identificacao, AnaliseCompletaDto analise, CancellationToken cancellationToken = default)
        {
            Chamada(autorId, cancellationToken);
            if (RespostaAtualizacao is not null)
                return Task.FromResult(RespostaAtualizacao);
            var original = registros[(id, autorId)];
            var salvo = new RebanhoGravadoDto(id, Guid.NewGuid(), Guid.NewGuid());
            registros[(id, autorId)] = original with
            {
                Nome = identificacao.Nome, Propriedade = identificacao.Propriedade,
                Versao = salvo.Versao, UltimoScoreId = salvo.UltimoScoreId,
                AtualizadoEmUtc = analise.CalculadaEmUtc, Analise = analise
            };
            historicos[(salvo.UltimoScoreId, autorId)] = new(salvo.UltimoScoreId, autorId, id,
                identificacao.Nome, identificacao.Propriedade, analise.CalculadaEmUtc, analise);
            Gravacoes++;
            return Task.FromResult(new ResultadoGravacaoRebanho(EstadoGravacaoRebanho.Sucesso, salvo));
        }

        public Task<EstadoGravacaoRebanho> ExcluirAsync(Guid id, string autorId, Guid versaoEsperada, CancellationToken cancellationToken = default)
        {
            Chamada(autorId, cancellationToken);
            if (RespostaExclusao is { } estado)
                return Task.FromResult(estado);
            registros.Remove((id, autorId));
            Gravacoes++;
            return Task.FromResult(EstadoGravacaoRebanho.Sucesso);
        }

        public Task<RebanhoPersistidoDto?> ConsultarAsync(Guid id, string autorId, CancellationToken cancellationToken = default)
        {
            Chamada(autorId, cancellationToken);
            return Task.FromResult(registros.GetValueOrDefault((id, autorId)));
        }

        public Task<PaginaRebanhosDto> ListarAsync(string autorId, int pagina, CancellationToken cancellationToken = default)
        {
            Chamada(autorId, cancellationToken);
            var itens = registros.Where(r => r.Key.Autor == autorId).Select(r => r.Value)
                .Select(r => new ItemListaRebanhosDto(r.Id, r.Nome, r.Propriedade, r.AtualizadoEmUtc, r.Analise.Score.Total, r.Analise.Score.Classificacao)).ToArray();
            return Task.FromResult(new PaginaRebanhosDto(pagina, 20, itens.Length, Array.AsReadOnly(itens)));
        }

        public Task<Guid> SalvarAvulsoAsync(string autorId, AnaliseCompletaDto analise, CancellationToken cancellationToken = default)
        {
            Chamada(autorId, cancellationToken);
            var id = Guid.NewGuid();
            historicos[(id, autorId)] = new(id, autorId, null, null, null, analise.CalculadaEmUtc, analise);
            return Task.FromResult(id);
        }

        public Task<ScoreHistoricoDto?> ConsultarDoAutorAsync(Guid id, string autorId, CancellationToken cancellationToken = default)
        {
            Chamada(autorId, cancellationToken);
            return Task.FromResult(historicos.GetValueOrDefault((id, autorId)));
        }
    }
}
