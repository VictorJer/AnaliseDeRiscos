using ScoreRebanho.Application.Contas;
using ScoreRebanho.Web.Comandos;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class InterpretarComandoContaTests
{
    [TestMethod]
    [DataRow(PerfisNegocio.Pecuarista)]
    [DataRow(PerfisNegocio.Cooperativa)]
    public void AceitaSomentePerfisConhecidosENormalizaEspacosDoEmail(string perfil)
    {
        Assert.IsTrue(ComandoContaLocal.TryParse(["contas", "definir-perfil", " pessoa@example.com ", perfil], out var comando));
        Assert.IsInstanceOfType<DefinirPerfilLocal>(comando);
        Assert.AreEqual("pessoa@example.com", comando.Email);
        Assert.AreEqual(perfil, ((DefinirPerfilLocal)comando).Perfil);
    }

    [TestMethod]
    public void ConcessaoAdministrativaNaoRecebePerfilOuPapelLivre()
    {
        Assert.IsTrue(ComandoContaLocal.TryParse(["contas", "conceder-administrador", "pessoa@example.com"], out var comando));
        Assert.IsInstanceOfType<ConcederAdministradorLocal>(comando);
        Assert.IsFalse(ComandoContaLocal.TryParse(["contas", "conceder-administrador", "pessoa@example.com", "OutroPapel"], out _));
    }

    [TestMethod]
    [DataRow("Administrador")]
    [DataRow("cooperativa")]
    [DataRow("")]
    public void RecusaPerfilNaoEspecificado(string perfil) =>
        Assert.IsFalse(ComandoContaLocal.TryParse(["contas", "definir-perfil", "pessoa@example.com", perfil], out _));

    [TestMethod]
    public void RecusaArgumentosAusentesDesconhecidosEEmailsInvalidos()
    {
        foreach (var argumentos in new string[][]
        {
            [], ["contas"], ["contas", "definir-perfil"],
            ["contas", "definir-perfil", "pessoa@example.com"],
            ["contas", "remover", "pessoa@example.com"],
            ["contas", "definir-perfil", "invalido", "Cooperativa"],
            ["contas", "definir-perfil", "Pessoa <pessoa@example.com>", "Cooperativa"],
            ["contas", "conceder-administrador", new string('a', 245) + "@example.com"]
        }) Assert.IsFalse(ComandoContaLocal.TryParse(argumentos, out _));
    }
}
