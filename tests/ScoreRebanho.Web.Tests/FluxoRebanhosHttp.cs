using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Web.Tests;

internal static class FluxoRebanhosHttp
{
    internal static async Task<string> CriarContaAsync(AplicacaoTeste app, HttpClient cliente,
        string perfil = PerfisNegocio.Pecuarista, bool administrador = false)
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var token = await TokenAsync(cliente, "/Conta/Cadastrar");
        var resposta = await cliente.PostAsync("/Conta/Cadastrar", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Senha"] = "Senha!123", ["ConfirmarSenha"] = "Senha!123",
            ["__RequestVerificationToken"] = token
        }));
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByEmailAsync(email);
        Assert.IsNotNull(usuario);
        if (perfil == PerfisNegocio.Cooperativa)
            Assert.IsTrue((await usuarios.ReplaceClaimAsync(usuario,
                new(PerfisNegocio.TipoClaim, PerfisNegocio.Pecuarista), new(PerfisNegocio.TipoClaim, perfil))).Succeeded);
        if (administrador)
        {
            var papeis = escopo.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            Assert.IsTrue((await papeis.CreateAsync(new("Administrador"))).Succeeded);
            Assert.IsTrue((await usuarios.AddToRoleAsync(usuario, "Administrador")).Succeeded);
        }
        return usuario.Id;
    }

    internal static Dictionary<string, string> Formulario(string nome = "Lote serrano") => new()
    {
        ["Nome"] = nome, ["Propriedade"] = "Fazenda Boa Vista",
        ["Entradas.AreaHectares"] = "100", ["Entradas.Pastagem"] = "1", ["Entradas.Posse"] = "1",
        ["Entradas.PastoPronto"] = "true", ["Entradas.RotacaoAtiva"] = "false",
        ["Entradas.QuantidadeAnimais"] = "100", ["Entradas.PesoCompraKg"] = "450",
        ["Entradas.PrecoCompraPorKgReais"] = "10", ["Entradas.DiasPastagem"] = "180",
        ["Entradas.RacaPuraCorte"] = "false", ["Entradas.VacinaCarbunculo"] = "true",
        ["Entradas.ControleCarrapato"] = "true", ["Entradas.Vermifugo"] = "true",
        ["Entradas.ModificadorOrganico"] = "false"
    };

    internal static async Task<Guid> CriarRebanhoAsync(HttpClient cliente, string nome = "Lote serrano")
    {
        var resposta = await PostAsync(cliente, "/Rebanhos/Cadastrar", Formulario(nome), "/Rebanhos/Cadastrar");
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode, await resposta.Content.ReadAsStringAsync());
        return Guid.Parse(resposta.Headers.Location!.OriginalString.Split('/').Last());
    }

    internal static async Task<HttpResponseMessage> PostAsync(HttpClient cliente, string destino,
        Dictionary<string, string> dados, string origem)
    {
        dados["__RequestVerificationToken"] = await TokenAsync(cliente, origem);
        return await cliente.PostAsync(destino, new FormUrlEncodedContent(dados));
    }

    internal static async Task<string> TokenAsync(HttpClient cliente, string caminho) =>
        Campo(await cliente.GetStringAsync(caminho), "__RequestVerificationToken");

    internal static string Campo(string html, string nome)
    {
        var campo = Regex.Match(html, "<input[^>]*name=\"" + Regex.Escape(nome) + "\"[^>]*>");
        Assert.IsTrue(campo.Success, $"Campo ausente: {nome}");
        return WebUtility.HtmlDecode(Regex.Match(campo.Value, "value=\"([^\"]*)\"").Groups[1].Value);
    }

    internal static async Task<int> ContarHistoricosAsync(AplicacaoTeste app)
    {
        await using var escopo = app.Services.CreateAsyncScope();
        return await escopo.ServiceProvider.GetRequiredService<ContasDbContext>()
            .Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM ScoresHistoricos").SingleAsync();
    }

    internal static async Task<RebanhoPersistidoDto> RebanhoAsync(AplicacaoTeste app, Guid id, string autor)
    {
        await using var escopo = app.Services.CreateAsyncScope();
        return (await escopo.ServiceProvider.GetRequiredService<IRepositorioRebanhos>().ConsultarAsync(id, autor))!;
    }
}
