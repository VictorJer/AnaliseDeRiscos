using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class AlterarSenhaHttpTests
{
    private const string SenhaAntiga = "Senha!123";
    private const string SenhaNova = "NovaSenha!456";

    [TestMethod]
    public async Task AlteracaoValidaAtualizaSomenteContaAutenticadaEMantemSessao()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autorId = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        using var outraConta = app.CriarCliente();
        var outroId = await FluxoRebanhosHttp.CriarContaAsync(app, outraConta);
        string email;
        string? stampOriginal;
        await using (var escopo = app.Services.CreateAsyncScope())
        {
            var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var usuario = await usuarios.FindByIdAsync(autorId);
            Assert.IsNotNull(usuario);
            email = usuario.Email!;
            stampOriginal = usuario.SecurityStamp;
        }
        var dados = Dados();
        dados["Id"] = outroId;
        dados["UsuarioId"] = outroId;
        dados["Email"] = "forjado@example.com";
        dados["Perfil"] = "Cooperativa";
        dados["Role"] = "Administrador";
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Conta/AlterarSenha", dados, "/Conta/AlterarSenha");
        Assert.AreEqual(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.AreEqual("/Conta/AlterarSenha", resposta.Headers.Location?.OriginalString);
        Assert.IsTrue(resposta.Headers.GetValues("Set-Cookie").Any(c => c.StartsWith("ScoreRebanho.Sessao=", StringComparison.Ordinal)));
        var pagina = await cliente.GetAsync(resposta.Headers.Location);
        Assert.AreEqual(HttpStatusCode.OK, pagina.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await pagina.Content.ReadAsStringAsync()), "Senha alterada com sucesso.");
        Assert.IsTrue(pagina.Headers.CacheControl?.NoStore);
        Assert.DoesNotContain("Senha alterada com sucesso.", WebUtility.HtmlDecode(await cliente.GetStringAsync("/Conta/AlterarSenha")));
        Assert.AreEqual(HttpStatusCode.OK, (await cliente.GetAsync("/Rebanhos")).StatusCode);
        await using (var escopo = app.Services.CreateAsyncScope())
        {
            var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var usuario = await usuarios.FindByIdAsync(autorId);
            var outro = await usuarios.FindByIdAsync(outroId);
            Assert.IsNotNull(usuario);
            Assert.IsNotNull(outro);
            Assert.AreEqual(email, usuario.Email);
            Assert.AreNotEqual(stampOriginal, usuario.SecurityStamp);
            Assert.IsTrue(await usuarios.CheckPasswordAsync(usuario, SenhaNova));
            Assert.IsFalse(await usuarios.CheckPasswordAsync(usuario, SenhaAntiga));
            Assert.IsTrue(await usuarios.CheckPasswordAsync(outro, SenhaAntiga));
            Assert.IsEmpty(await usuarios.GetRolesAsync(usuario));
            Assert.HasCount(1, await usuarios.GetClaimsAsync(usuario));
        }
        using var antigoLogin = app.CriarCliente();
        var recusado = await EntrarAsync(antigoLogin, email, SenhaAntiga);
        Assert.AreEqual(HttpStatusCode.OK, recusado.StatusCode);
        StringAssert.Contains(WebUtility.HtmlDecode(await recusado.Content.ReadAsStringAsync()), "E-mail ou senha inválidos.");
        using var novoLogin = app.CriarCliente();
        Assert.AreEqual(HttpStatusCode.Redirect, (await EntrarAsync(novoLogin, email, SenhaNova)).StatusCode);
    }

    [TestMethod]
    [DataRow("Incorreta!123", SenhaNova, SenhaNova, "A senha atual está incorreta.")]
    [DataRow(SenhaAntiga, "Abcdefg!", "Abcdefg!", "A senha deve ter um número.")]
    [DataRow(SenhaAntiga, SenhaNova, "OutraSenha!456", "As senhas não conferem.")]
    [DataRow("", SenhaNova, SenhaNova, "Informe sua senha atual.")]
    [DataRow(SenhaAntiga, "", "", "Informe a nova senha.")]
    [DataRow(SenhaAntiga, "Curta1!", "Curta1!", "entre 8 e 128")]
    public async Task EntradasInvalidasNaoAlteramCredenciaisNemReexibemSenhas(string atual, string nova, string confirmacao, string mensagem)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var dados = new Dictionary<string, string>
        {
            ["SenhaAtual"] = atual, ["NovaSenha"] = nova, ["ConfirmarNovaSenha"] = confirmacao
        };
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Conta/AlterarSenha", dados, "/Conta/AlterarSenha");
        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);
        var html = await resposta.Content.ReadAsStringAsync();
        StringAssert.Contains(WebUtility.HtmlDecode(html), mensagem);
        foreach (var campo in dados.Keys.Where(c => c != "__RequestVerificationToken"))
            Assert.AreEqual("", FluxoRebanhosHttp.Campo(html, campo));
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByIdAsync(autor);
        Assert.IsNotNull(usuario);
        Assert.IsTrue(await usuarios.CheckPasswordAsync(usuario, SenhaAntiga));
    }

    [TestMethod]
    public async Task SenhaMuitoLongaERecusadaSemAlterarConta()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        var dados = Dados();
        dados["NovaSenha"] = dados["ConfirmarNovaSenha"] = "Aa1!" + new string('a', 125);
        var resposta = await FluxoRebanhosHttp.PostAsync(cliente, "/Conta/AlterarSenha", dados, "/Conta/AlterarSenha");
        StringAssert.Contains(WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync()), "entre 8 e 128");
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByIdAsync(autor);
        Assert.IsNotNull(usuario);
        Assert.IsTrue(await usuarios.CheckPasswordAsync(usuario, SenhaAntiga));
    }

    [TestMethod]
    public async Task ExigeAutenticacaoECsrfEGetNaoAlteraSenha()
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var anonimo = await cliente.GetAsync("/Conta/AlterarSenha");
        Assert.AreEqual(HttpStatusCode.Redirect, anonimo.StatusCode);
        StringAssert.StartsWith(anonimo.Headers.Location!.AbsolutePath, "/Conta/Entrar");
        var autor = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.PostAsync("/Conta/AlterarSenha", new FormUrlEncodedContent(Dados()))).StatusCode);
        var pagina = await cliente.GetAsync("/Conta/AlterarSenha?SenhaAtual=Senha!123&NovaSenha=Forjada!456&ConfirmarNovaSenha=Forjada!456");
        Assert.AreEqual(HttpStatusCode.OK, pagina.StatusCode);
        Assert.AreEqual("", FluxoRebanhosHttp.Campo(await pagina.Content.ReadAsStringAsync(), "NovaSenha"));
        await using var escopo = app.Services.CreateAsyncScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await usuarios.FindByIdAsync(autor);
        Assert.IsNotNull(usuario);
        Assert.IsTrue(await usuarios.CheckPasswordAsync(usuario, SenhaAntiga));
    }

    private static Dictionary<string, string> Dados() => new()
    {
        ["SenhaAtual"] = SenhaAntiga, ["NovaSenha"] = SenhaNova, ["ConfirmarNovaSenha"] = SenhaNova
    };

    private static Task<HttpResponseMessage> EntrarAsync(HttpClient cliente, string email, string senha) =>
        FluxoRebanhosHttp.PostAsync(cliente, "/Conta/Entrar", new()
        {
            ["Email"] = email, ["Senha"] = senha
        }, "/Conta/Entrar");
}
