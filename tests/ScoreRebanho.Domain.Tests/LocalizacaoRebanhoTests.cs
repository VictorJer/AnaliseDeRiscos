using ScoreRebanho.Domain.Rebanhos;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public sealed class LocalizacaoRebanhoTests
{
    [TestMethod]
    public void AceitaLimitesEZeroSemArredondar()
    {
        Assert.AreEqual(-90m, new LocalizacaoRebanho(-90, -180).Latitude);
        Assert.AreEqual(180m, new LocalizacaoRebanho(90, 180).Longitude);
        Assert.AreEqual(0m, new LocalizacaoRebanho(0, 0).Latitude);
        Assert.AreEqual(-27.12345678m, new LocalizacaoRebanho(-27.12345678m, -49).Latitude);
    }

    [TestMethod]
    public void RecusaCoordenadasForaDosLimites()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LocalizacaoRebanho(-90.0001m, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LocalizacaoRebanho(90.0001m, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LocalizacaoRebanho(0, -180.0001m));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LocalizacaoRebanho(0, 180.0001m));
    }
}
