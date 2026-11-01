using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Infrastructure.Persistencia;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public sealed class ComandosContasProcessoTests
{
    [TestMethod]
    public async Task AjudaEArgumentosInvalidosTerminamSemAbrirBancoOuServidor()
    {
        var ajuda = await ExecutarAsync("conexao propositalmente invalida", "contas", "ajuda");
        Assert.AreEqual(0, ajuda.Codigo);
        StringAssert.Contains(ajuda.Saida, "definir-perfil");
        var invalido = await ExecutarAsync("conexao propositalmente invalida", "contas", "definir-perfil", "pessoa@example.com", "Administrador");
        Assert.AreEqual(2, invalido.Codigo);
        StringAssert.Contains(invalido.Erro, "Pecuarista|Cooperativa");
    }

    [TestMethod]
    [DataRow("definir-perfil")]
    [DataRow("conceder-administrador")]
    public async Task ComandoRealAtualizaSomenteContaExistenteNoBancoConfigurado(string operacao)
    {
        await using var app = new AplicacaoTeste();
        using var cliente = app.CriarCliente();
        var id = await FluxoRebanhosHttp.CriarContaAsync(app, cliente);
        string email;
        string conexao;
        await using (var escopo = app.Services.CreateAsyncScope())
        {
            var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            email = (await usuarios.FindByIdAsync(id))!.Email!;
            conexao = escopo.ServiceProvider.GetRequiredService<ContasDbContext>().Database.GetConnectionString()!;
        }
        var argumentos = operacao == "definir-perfil"
            ? new[] { "contas", operacao, email, PerfisNegocio.Cooperativa }
            : new[] { "contas", operacao, email };
        var resposta = await ExecutarAsync(conexao, argumentos);
        Assert.AreEqual(0, resposta.Codigo, resposta.Erro + resposta.Saida);
        await using var consulta = app.Services.CreateAsyncScope();
        var manager = consulta.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var usuario = await manager.FindByIdAsync(id);
        Assert.IsNotNull(usuario);
        var perfil = (await manager.GetClaimsAsync(usuario)).Single(c => c.Type == PerfisNegocio.TipoClaim).Value;
        Assert.AreEqual(operacao == "definir-perfil" ? PerfisNegocio.Cooperativa : PerfisNegocio.Pecuarista, perfil);
        Assert.AreEqual(operacao == "conceder-administrador", await manager.IsInRoleAsync(usuario, "Administrador"));
        var inexistente = await ExecutarAsync(conexao, "contas", "conceder-administrador", "ausente@example.com");
        Assert.AreEqual(3, inexistente.Codigo);
        Assert.IsNull(await manager.FindByEmailAsync("ausente@example.com"));
    }

    private static async Task<(int Codigo, string Saida, string Erro)> ExecutarAsync(string conexao, params string[] argumentos)
    {
        var assembly = typeof(Program).Assembly.Location;
        var inicio = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(assembly)!
        };
        inicio.ArgumentList.Add(assembly);
        foreach (var argumento in argumentos) inicio.ArgumentList.Add(argumento);
        inicio.Environment["ConnectionStrings__Contas"] = conexao;
        inicio.Environment["DOTNET_ENVIRONMENT"] = "Testing";
        inicio.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
        using var processo = Process.Start(inicio)!;
        var saida = processo.StandardOutput.ReadToEndAsync();
        var erro = processo.StandardError.ReadToEndAsync();
        using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await processo.WaitForExitAsync(limite.Token);
            return (processo.ExitCode, await saida, await erro);
        }
        finally
        {
            if (!processo.HasExited) processo.Kill(entireProcessTree: true);
        }
    }
}
