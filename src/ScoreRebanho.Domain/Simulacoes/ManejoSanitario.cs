namespace ScoreRebanho.Domain.Simulacoes;

public sealed record ManejoSanitario(
    bool VacinaCarbunculo,
    bool ControleCarrapato,
    bool Vermifugo,
    bool ModificadorOrganico);
