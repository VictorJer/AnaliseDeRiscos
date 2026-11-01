using System.Net;
using System.Text.RegularExpressions;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class LayoutHttpTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task RecursosDoLayoutSaoServidosComTipoCorreto(bool autenticado)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        if (autenticado) await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var pagina = await cliente.GetAsync(autenticado ? "/Simulacoes/Nova" : "/");
        Assert.AreEqual(HttpStatusCode.OK, pagina.StatusCode);
        var html = WebUtility.HtmlDecode(await pagina.Content.ReadAsStringAsync());
        var recursos = Regex.Matches(html, "(?:src|href)=\"(?<url>/[^\"]+\\.(?:css|js)(?:\\?[^\"]*)?)\"")
            .Select(m => m.Groups["url"].Value).Distinct().ToArray();
        Assert.IsGreaterThanOrEqualTo(5, recursos.Length, "O layout deve expor seus recursos locais.");
        foreach (var url in recursos)
        {
            var resposta = await cliente.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode, url);
            var tipo = resposta.Content.Headers.ContentType?.MediaType;
            if (url.Split('?')[0].EndsWith(".css", StringComparison.Ordinal))
                Assert.AreEqual("text/css", tipo, url);
            else
                Assert.IsTrue(tipo is "text/javascript" or "application/javascript", url);
            Assert.IsGreaterThan(0, (await resposta.Content.ReadAsByteArrayAsync()).Length, url);
        }
    }
}
