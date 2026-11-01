using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Application.Tests;

[TestClass]
public sealed class CadastrarContaTests
{
    [TestMethod]
    [DataRow("")]
    [DataRow("invalido")]
    [DataRow("Nome <nome@example.com>")]
    public async Task EmailInvalidoNaoChamaPersistencia(string email)
    {
        var persistencia = new CadastroMemoria();
        var resultado = await new CadastrarConta(persistencia).ExecutarAsync(new() { Email = email, Senha = "Senha!123" });
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsNull(persistencia.DadosRecebidos);
    }

    [TestMethod]
    public async Task CadastroNormalizaEmailSemModificarSenha()
    {
        var persistencia = new CadastroMemoria();
        var resultado = await new CadastrarConta(persistencia).ExecutarAsync(new() { Email = " pessoa@example.com ", Senha = " Senha!123 " });
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual("id-conta", resultado.Value.Id);
        Assert.AreEqual("pessoa@example.com", persistencia.DadosRecebidos!.Email);
        Assert.AreEqual(" Senha!123 ", persistencia.DadosRecebidos.Senha);
    }

    [TestMethod]
    public async Task FalhaEsperadaPreservaMensagemECampo()
    {
        var persistencia = new CadastroMemoria { Resultado = new(null, [new("Senha", "Senha inválida.")]) };
        var resultado = await new CadastrarConta(persistencia).ExecutarAsync(new() { Email = "pessoa@example.com", Senha = "abc" });
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Senha", resultado.Errors[0].Metadata["Campo"]);
    }

    [TestMethod]
    public async Task PropagaCancelamentoParaPersistencia()
    {
        using var cancelamento = new CancellationTokenSource();
        var persistencia = new CadastroMemoria();
        await new CadastrarConta(persistencia).ExecutarAsync(new() { Email = "pessoa@example.com", Senha = "Senha!123" }, cancelamento.Token);
        Assert.AreEqual(cancelamento.Token, persistencia.TokenRecebido);
    }

    private sealed class CadastroMemoria : ICadastroContas
    {
        public CadastrarContaDto? DadosRecebidos { get; private set; }
        public CancellationToken TokenRecebido { get; private set; }
        public CadastroPersistido Resultado { get; init; } = new("id-conta", []);
        public Task<CadastroPersistido> CadastrarAsync(CadastrarContaDto dados, CancellationToken cancellationToken)
        {
            DadosRecebidos = dados;
            TokenRecebido = cancellationToken;
            return Task.FromResult(Resultado);
        }
    }
}
