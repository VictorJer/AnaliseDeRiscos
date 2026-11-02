using System.Net;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class LocalizacaoHttpTests
{
    [TestMethod]
    public async Task CoordenadasPtBrPersistemNaEdicaoERecusamParIncompleto()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var dados = FluxoRebanhosHttp.Formulario();
        dados["Latitude"] = "-27,912345";
        dados["Longitude"] = "-49,812345";
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Rebanhos/Cadastrar", dados, "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        var id = Guid.Parse(resposta.Headers.Location!.OriginalString.Split('/').Last());
        var salvo = await FluxoRebanhosHttp.RebanhoAsync(app, id, autor);
        Assert.AreEqual(-27.912345m, salvo.Latitude);
        var html = await cliente.GetStringAsync($"/Rebanhos/Editar/{id}");
        Assert.AreEqual("-27,912345", FluxoRebanhosHttp.Campo(html, "Latitude"));
        dados["Id"] = id.ToString();
        dados["Versao"] = salvo.Versao.ToString();
        dados["Longitude"] = "";
        var invalido = await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/Editar/{id}", dados, $"/Rebanhos/Editar/{id}");
        Assert.AreEqual(HttpStatusCode.OK, invalido.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await invalido.Content.ReadAsStringAsync()), "Informe latitude e longitude");
        Assert.AreEqual(1, await FluxoRebanhosHttp.ContarHistoricosAsync(app));
        dados["Latitude"] = "";
        var removido = await FluxoRebanhosHttp.PostAsync(cliente, $"/Rebanhos/Editar/{id}", dados, $"/Rebanhos/Editar/{id}");
        Assert.AreEqual(HttpStatusCode.Redirect, removido.StatusCode);
        Assert.IsNull((await FluxoRebanhosHttp.RebanhoAsync(app, id, autor)).Latitude);
    }
}
