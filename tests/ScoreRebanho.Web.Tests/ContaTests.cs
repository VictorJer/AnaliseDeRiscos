using System.Net;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class ContaTests
{
    private const string Email = "pessoa@example.com";
    private const string Senha = "Senha!123";

    [TestMethod]
    public async Task CadastroPersisteHashPerfilSeguroEIniciaSessao()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var resposta = await CadastrarAsync(cliente, new() { ["Perfil"] = "Cooperativa", ["Role"] = "Administrador" });
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.IsTrue(resposta.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.IsTrue(cookies.Any(c => c.StartsWith("ScoreRebanho.Sessao=")));

        using var escopo = app.Services.CreateScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByEmailAsync(Email);
        Assert.IsNotNull(usuario);
        Assert.AreNotEqual(Senha, usuario.PasswordHash);
        Assert.IsTrue(await usuarios.CheckPasswordAsync(usuario, Senha));
        var claims = await usuarios.GetClaimsAsync(usuario);
        Assert.AreEqual(1, claims.Count(c => c.Type == PerfisNegocio.TipoClaim));
        Assert.IsTrue(claims.Any(c => c.Type == PerfisNegocio.TipoClaim && c.Value == PerfisNegocio.Pecuarista));
        Assert.IsEmpty(await usuarios.GetRolesAsync(usuario));
        StringAssert.Contains(await cliente.GetStringAsync("/"), "Você está conectado");
    }

    [TestMethod]
    public async Task CadastroComConfirmacaoDiferenteNaoPersiste()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var resposta = await CadastrarAsync(cliente, new() { ["ConfirmarSenha"] = "Diferente!123" });
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync()), "As senhas não conferem.");
        using var escopo = app.Services.CreateScope();
        Assert.AreEqual(0, await escopo.ServiceProvider.GetRequiredService<ContasDbContext>().Users.CountAsync());
    }

    [TestMethod]
    public async Task SenhaFracaNaoPersisteContaNemPerfil()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var resposta = await CadastrarAsync(cliente, new() { ["Senha"] = "abcdefgh", ["ConfirmarSenha"] = "abcdefgh" });
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        using var escopo = app.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ContasDbContext>();
        Assert.AreEqual(0, await contexto.Users.CountAsync());
        Assert.AreEqual(0, await contexto.UserClaims.CountAsync());
    }

    [TestMethod]
    public async Task CadastroDuplicadoNaoCriaOutraConta()
    {
        await using var app = new AplicacaoTeste();
        using var primeiro = app.CriarCliente();
        await CadastrarAsync(primeiro);
        using var segundo = app.CriarCliente();
        var resposta = await CadastrarAsync(segundo);
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        using var escopo = app.Services.CreateScope();
        Assert.AreEqual(1, await escopo.ServiceProvider.GetRequiredService<ContasDbContext>().Users.CountAsync());
    }

    [TestMethod]
    public async Task LoginComSenhaErradaNaoAutentica()
    {
        await using var app = new AplicacaoTeste();
        using var cadastro = app.CriarCliente();
        await CadastrarAsync(cadastro);
        using var cliente = app.CriarCliente();
        var resposta = await EntrarAsync(cliente, "Incorreta!123");
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync()), "E-mail ou senha inválidos.");
        Assert.IsFalse(resposta.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(c => c.StartsWith("ScoreRebanho.Sessao=")));
    }

    [TestMethod]
    public async Task LoginRecusaRedirecionamentoExternoELogoutEncerraSessao()
    {
        await using var app = new AplicacaoTeste();
        using var cadastro = app.CriarCliente();
        await CadastrarAsync(cadastro);
        using var cliente = app.CriarCliente();
        var resposta = await EntrarAsync(cliente, Senha, "https://example.org/fora");
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.AreEqual("/", resposta.Headers.Location?.OriginalString);
        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.GetAsync("/Conta/Sair")).StatusCode);
        StringAssert.Contains(await cliente.GetStringAsync("/"), "Você está conectado");
        var token = await ObterTokenAsync(cliente, "/");
        var logout = await cliente.PostAsync("/Conta/Sair", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
        Assert.AreEqual(HttpStatusCode.Redirect, logout.StatusCode);
        StringAssert.Contains(await cliente.GetStringAsync("/"), "Cadastrar");

    }

    [TestMethod]
    public async Task CincoTentativasInvalidasBloqueiamTemporariamente()
    {
        await using var app = new AplicacaoTeste();
        using var cadastro = app.CriarCliente();
        await CadastrarAsync(cadastro);
        using var cliente = app.CriarCliente();
        for (var tentativa = 0; tentativa < 5; tentativa++)
            await EntrarAsync(cliente, "Incorreta!123");
        var resposta = await EntrarAsync(cliente, Senha);
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync()), "Acesso temporariamente bloqueado");
    }

    [TestMethod]
    [DataRow("/Conta/Cadastrar")]
    [DataRow("/Conta/Entrar")]
    [DataRow("/Conta/Sair")]
    public async Task PostSemCsrfERecusado(string caminho)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        if (caminho.EndsWith("Sair"))
            await CadastrarAsync(cliente);
        var resposta = await cliente.PostAsync(caminho, new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task MapeamentoDeCadastroEValido()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
        Assert.AreEqual(HttpStatusCode.OK, (await cliente.GetAsync("/Conta/Cadastrar")).StatusCode);
    }

    [TestMethod]
    public async Task MigrationCorrespondeAoModeloPersistido()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        using var escopo = app.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ContasDbContext>();
        Assert.IsFalse(contexto.Database.HasPendingModelChanges());
        Assert.AreEqual(1, (await contexto.Database.GetAppliedMigrationsAsync()).Count());
    }

    private static async Task<HttpResponseMessage> CadastrarAsync(HttpClient cliente, Dictionary<string, string>? substituicoes = null)
    {
        var dados = new Dictionary<string, string>
        {
            ["Email"] = Email, ["Senha"] = Senha, ["ConfirmarSenha"] = Senha,
            ["__RequestVerificationToken"] = await ObterTokenAsync(cliente, "/Conta/Cadastrar")
        };
        if (substituicoes is not null)
            foreach (var item in substituicoes)
                dados[item.Key] = item.Value;
        return await cliente.PostAsync("/Conta/Cadastrar", new FormUrlEncodedContent(dados));
    }

    private static async Task<HttpResponseMessage> EntrarAsync(HttpClient cliente, string senha, string returnUrl = "/")
    {
        var dados = new Dictionary<string, string>
        {
            ["Email"] = Email, ["Senha"] = senha, ["ReturnUrl"] = returnUrl,
            ["__RequestVerificationToken"] = await ObterTokenAsync(cliente, "/Conta/Entrar")
        };
        return await cliente.PostAsync("/Conta/Entrar", new FormUrlEncodedContent(dados));
    }

    private static async Task<string> ObterTokenAsync(HttpClient cliente, string caminho)
    {
        var html = await cliente.GetStringAsync(caminho);
        var campo = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*>");
        Assert.IsTrue(campo.Success, "A resposta deve conter um token CSRF.");
        return WebUtility.HtmlDecode(Regex.Match(campo.Value, "value=\"([^\"]+)\"").Groups[1].Value);
    }
}
