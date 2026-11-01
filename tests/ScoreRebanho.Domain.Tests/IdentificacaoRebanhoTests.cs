using ScoreRebanho.Domain.Rebanhos;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class IdentificacaoRebanhoTests
{
    [TestMethod]
    public void PreservaNomeEPropriedadeInformados()
    {
        var identificacao = new IdentificacaoRebanho("Lote de inverno", "Fazenda São José");
        Assert.AreEqual("Lote de inverno", identificacao.Nome);
        Assert.AreEqual("Fazenda São José", identificacao.Propriedade);
    }

    [TestMethod]
    public void RemoveEspacosDasExtremidadesSemAlterarEspacosInternos()
    {
        var identificacao = new IdentificacaoRebanho("  Lote  de inverno  ", "  Fazenda São José  ");
        Assert.AreEqual("Lote  de inverno", identificacao.Nome);
        Assert.AreEqual("Fazenda São José", identificacao.Propriedade);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t\r\n")]
    public void RejeitaNomeAusente(string? nome)
    {
        Assert.Throws<ArgumentException>(() => new IdentificacaoRebanho(nome!, "Fazenda São José"));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("\t\r\n")]
    public void RejeitaPropriedadeAusente(string? propriedade)
    {
        Assert.Throws<ArgumentException>(() => new IdentificacaoRebanho("Lote de inverno", propriedade!));
    }

    [TestMethod]
    public void AceitaTamanhosMaximosAposRemoverEspacos()
    {
        var identificacao = new IdentificacaoRebanho(" " + new string('N', 100) + " ", " " + new string('P', 150) + " ");
        Assert.AreEqual(100, identificacao.Nome.Length);
        Assert.AreEqual(150, identificacao.Propriedade.Length);
    }

    [TestMethod]
    public void RejeitaNomeAcimaDoLimite()
    {
        var erro = Assert.Throws<ArgumentException>(() => new IdentificacaoRebanho(new string('N', 101), "Fazenda São José"));
        Assert.AreEqual("nome", erro.ParamName);
    }

    [TestMethod]
    public void RejeitaPropriedadeAcimaDoLimite()
    {
        var erro = Assert.Throws<ArgumentException>(() => new IdentificacaoRebanho("Lote de inverno", new string('P', 151)));
        Assert.AreEqual("propriedade", erro.ParamName);
    }

    [TestMethod]
    public void PermiteIdentificacoesRepetidas()
    {
        var primeira = new IdentificacaoRebanho("Lote de inverno", "Fazenda São José");
        var segunda = new IdentificacaoRebanho("Lote de inverno", "Fazenda São José");
        Assert.AreEqual(primeira, segunda);
    }
}
