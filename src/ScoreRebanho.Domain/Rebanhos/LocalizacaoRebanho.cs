namespace ScoreRebanho.Domain.Rebanhos;

public sealed record LocalizacaoRebanho
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }
    public LocalizacaoRebanho(decimal latitude, decimal longitude)
    {
        if (latitude is < -90m or > 90m)
            throw new ArgumentOutOfRangeException(nameof(latitude), "A latitude deve estar entre -90 e 90 graus.");
        if (longitude is < -180m or > 180m)
            throw new ArgumentOutOfRangeException(nameof(longitude), "A longitude deve estar entre -180 e 180 graus.");
        Latitude = latitude;
        Longitude = longitude;
    }
}
