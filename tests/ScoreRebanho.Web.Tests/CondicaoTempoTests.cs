using ScoreRebanho.Web.ViewModels.Acompanhamento;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class CondicaoTempoTests
{
    [TestMethod]
    [DataRow(0, "sol")]
    [DataRow(2, "sol-nuvem")]
    [DataRow(3, "nuvem")]
    [DataRow(45, "neblina")]
    [DataRow(63, "chuva")]
    [DataRow(73, "neve")]
    [DataRow(95, "tempestade")]
    [DataRow(97, "tempestade")]
    public void CodigoDaFonteSelecionaFamiliaVisualCorreta(int codigo, string icone)
    {
        var condicao = CondicaoTempoViewModel.DoCodigo(codigo);
        Assert.AreEqual(icone, condicao.Icone);
        Assert.IsFalse(string.IsNullOrWhiteSpace(condicao.Descricao));
    }

    [TestMethod]
    public void CodigoDesconhecidoOuAusenteTemDescricaoExplicita()
    {
        foreach (var codigo in new int?[] { null, -1, 999 })
        {
            var condicao = CondicaoTempoViewModel.DoCodigo(codigo);
            Assert.AreEqual("indisponivel", condicao.Icone);
            Assert.AreEqual("Condição não informada", condicao.Descricao);
        }
    }
}
