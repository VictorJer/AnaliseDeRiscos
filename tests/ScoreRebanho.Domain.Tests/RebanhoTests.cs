using ScoreRebanho.Domain.Rebanhos;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class RebanhoTests
{
    [TestMethod]
    public void CriacaoGeraIdentificadoresEPreservaAutoria()
    {
        var data = new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.FromHours(-3));
        var score = Guid.NewGuid();
        var rebanho = Rebanho.Criar("autor", new("Lote", "Fazenda"), score, data);
        Assert.AreNotEqual(Guid.Empty, rebanho.Id);
        Assert.AreNotEqual(Guid.Empty, rebanho.Versao);
        Assert.AreEqual("autor", rebanho.AutorId);
        Assert.AreEqual(score, rebanho.UltimoScoreId);
        Assert.AreEqual(TimeSpan.Zero, rebanho.CriadoEmUtc.Offset);
        Assert.AreEqual(data.ToUniversalTime(), rebanho.CriadoEmUtc);
        Assert.AreEqual(rebanho.CriadoEmUtc, rebanho.AtualizadoEmUtc);
    }

    [TestMethod]
    public void AtualizarTrocaVersaoEScoreSemAlterarAutoriaOuCriacao()
    {
        var data = DateTimeOffset.UnixEpoch;
        var rebanho = Rebanho.Criar("autor", new("Antes", "Fazenda"), Guid.NewGuid(), data);
        var id = rebanho.Id;
        var versao = rebanho.Versao;
        var novoScore = Guid.NewGuid();
        rebanho.Atualizar(new("Depois", "Outra fazenda"), novoScore, data.AddDays(1));
        Assert.AreEqual(id, rebanho.Id);
        Assert.AreEqual("autor", rebanho.AutorId);
        Assert.AreEqual(data, rebanho.CriadoEmUtc);
        Assert.AreEqual(data.AddDays(1), rebanho.AtualizadoEmUtc);
        Assert.AreNotEqual(versao, rebanho.Versao);
        Assert.AreEqual(novoScore, rebanho.UltimoScoreId);
        Assert.AreEqual("Depois", rebanho.Identificacao.Nome);
    }

    [TestMethod]
    public void RejeitaCriacaoSemAutorOuScoreEAtualizacaoSemScore()
    {
        Assert.Throws<ArgumentException>(() => Rebanho.Criar("", new("Lote", "Fazenda"), Guid.NewGuid(), DateTimeOffset.UnixEpoch));
        Assert.Throws<ArgumentException>(() => Rebanho.Criar("autor", new("Lote", "Fazenda"), Guid.Empty, DateTimeOffset.UnixEpoch));
        var rebanho = Rebanho.Criar("autor", new("Lote", "Fazenda"), Guid.NewGuid(), DateTimeOffset.UnixEpoch);
        var versao = rebanho.Versao;
        Assert.Throws<ArgumentException>(() => rebanho.Atualizar(new("Lote", "Fazenda"), Guid.Empty, DateTimeOffset.UnixEpoch));
        Assert.AreEqual(versao, rebanho.Versao);
    }
}
