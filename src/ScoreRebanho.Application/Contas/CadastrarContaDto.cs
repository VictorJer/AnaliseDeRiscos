namespace ScoreRebanho.Application.Contas;

public sealed class CadastrarContaDto
{
    public string Email { get; init; } = "";
    public string Senha { get; init; } = "";
}

public sealed record ContaCriadaDto(string Id);

public sealed record FalhaCadastro(string Campo, string Mensagem);

public sealed record CadastroPersistido(string? UsuarioId, IReadOnlyList<FalhaCadastro> Falhas);

public interface ICadastroContas
{
    Task<CadastroPersistido> CadastrarAsync(CadastrarContaDto dados, CancellationToken cancellationToken);
}
