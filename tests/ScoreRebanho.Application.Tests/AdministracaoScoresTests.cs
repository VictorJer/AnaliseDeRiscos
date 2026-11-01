using ScoreRebanho.Application.Administracao;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Tests.Rebanhos;

namespace ScoreRebanho.Application.Tests;

[TestClass]
public sealed class AdministracaoScoresTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public async Task PerfilDeNegocioNaoConcedeConsultaAdministrativa(string perfil)
    {
        var usuario = new CenarioRebanhos.UsuarioTeste { Conta = new("a", perfil) };
        var consulta = new ConsultaTeste();
        var lista = await new ListarScoresAdministrativos(usuario, consulta).ExecutarAsync(new());
        var detalhe = await new ConsultarScoreAdministrativo(usuario, consulta).ExecutarAsync(new(Guid.NewGuid()));
        Assert.AreEqual("NAO_AUTORIZADO", lista.Errors[0].Metadata["Codigo"]);
        Assert.AreEqual("NAO_AUTORIZADO", detalhe.Errors[0].Metadata["Codigo"]);
        Assert.AreEqual(0, consulta.Chamadas);
    }

    [TestMethod]
    public async Task ContaAusenteERecusadaAntesDeConsultarDados()
    {
        var usuario = new CenarioRebanhos.UsuarioTeste { Conta = null };
        var consulta = new ConsultaTeste();
        var lista = await new ListarScoresAdministrativos(usuario, consulta).ExecutarAsync(new());
        Assert.AreEqual("NAO_AUTENTICADO", lista.Errors[0].Metadata["Codigo"]);
        usuario.Conta = new("", null, true);
        var detalhe = await new ConsultarScoreAdministrativo(usuario, consulta).ExecutarAsync(new(Guid.NewGuid()));
        Assert.AreEqual("NAO_AUTENTICADO", detalhe.Errors[0].Metadata["Codigo"]);
        Assert.AreEqual(0, consulta.Chamadas);
    }

    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    [DataRow(null)]
    public async Task AdministradorConsultaSnapshotCompletoIndependenteDoPerfil(string? perfil)
    {
        var usuario = new CenarioRebanhos.UsuarioTeste { Conta = new("administrador", perfil, true) };
        var cenario = new CenarioRebanhos();
        var analise = cenario.Calcular.Executar(CenarioRebanhos.Entrada.Entradas).Value;
        var consulta = new ConsultaTeste
        {
            Relatorio = new(Guid.NewGuid(), "outro-autor", "outro@example.com", "Lote", "Fazenda", analise)
        };
        using var token = new CancellationTokenSource();
        var lista = await new ListarScoresAdministrativos(usuario, consulta).ExecutarAsync(new(2), token.Token);
        Assert.IsTrue(lista.IsSuccess);
        Assert.AreEqual(2, consulta.Pagina);
        var detalhe = await new ConsultarScoreAdministrativo(usuario, consulta).ExecutarAsync(new(consulta.Relatorio.Id), token.Token);
        Assert.IsTrue(detalhe.IsSuccess);
        Assert.AreSame(analise, detalhe.Value.Analise);
        Assert.AreEqual("outro-autor", detalhe.Value.AutorId);
        Assert.AreEqual(token.Token, consulta.Token);
        Assert.AreEqual(1, cenario.Relogio.Chamadas);
        usuario.Conta = usuario.Conta with { Administrador = false };
        Assert.IsTrue((await new ConsultarScoreAdministrativo(usuario, consulta).ExecutarAsync(new(consulta.Relatorio.Id))).IsFailed);
        Assert.AreEqual(2, consulta.Chamadas);
    }

    [TestMethod]
    public async Task PaginaInvalidaEIdsAusentesNaoEntregamDados()
    {
        var usuario = new CenarioRebanhos.UsuarioTeste { Conta = new("a", null, true) };
        var consulta = new ConsultaTeste();
        Assert.IsTrue((await new ListarScoresAdministrativos(usuario, consulta).ExecutarAsync(new(0))).IsFailed);
        Assert.IsTrue((await new ListarScoresAdministrativos(usuario, consulta).ExecutarAsync(new(-1))).IsFailed);
        var caso = new ConsultarScoreAdministrativo(usuario, consulta);
        Assert.AreEqual("NAO_ENCONTRADO", (await caso.ExecutarAsync(new(Guid.Empty))).Errors[0].Metadata["Codigo"]);
        Assert.AreEqual(0, consulta.Chamadas);
        Assert.AreEqual("NAO_ENCONTRADO", (await caso.ExecutarAsync(new(Guid.NewGuid()))).Errors[0].Metadata["Codigo"]);
    }

    [TestMethod]
    public async Task CancelamentoEFalhasInesperadasNaoSaoConvertidosEmSucesso()
    {
        var usuario = new CenarioRebanhos.UsuarioTeste { Conta = new("a", null, true) };
        var consulta = new ConsultaTeste();
        using var cancelamento = new CancellationTokenSource();
        cancelamento.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => new ListarScoresAdministrativos(usuario, consulta).ExecutarAsync(new(), cancelamento.Token));
        Assert.AreEqual(0, consulta.Chamadas);
        consulta.Falhar = true;
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new ConsultarScoreAdministrativo(usuario, consulta).ExecutarAsync(new(Guid.NewGuid())));
    }

    private sealed class ConsultaTeste : IConsultaScoresAdministrativos
    {
        internal RelatorioScoreAdministrativoDto? Relatorio { get; init; }
        internal int Chamadas { get; private set; }
        internal int Pagina { get; private set; }
        internal CancellationToken Token { get; private set; }
        internal bool Falhar { get; set; }
        public Task<PaginaScoresAdministrativosDto> ListarAsync(int pagina, CancellationToken cancellationToken = default)
        {
            Chamadas++;
            Pagina = pagina;
            Token = cancellationToken;
            return Task.FromResult(new PaginaScoresAdministrativosDto(pagina, 20, 0, []));
        }
        public Task<RelatorioScoreAdministrativoDto?> ConsultarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Chamadas++;
            Token = cancellationToken;
            if (Falhar) throw new InvalidOperationException("Falha simulada.");
            return Task.FromResult(Relatorio);
        }
    }
}
