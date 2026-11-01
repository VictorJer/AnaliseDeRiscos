using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using ScoreRebanho.Application.Contas;

namespace ScoreRebanho.Web.Comandos;

public abstract record ComandoContaLocal(string Email)
{
    public const string Ajuda = """
        Comandos locais para contas já cadastradas:
          contas definir-perfil <email> <Pecuarista|Cooperativa>
          contas conceder-administrador <email>
          contas ajuda
        Use a configuração ConnectionStrings:Contas do banco desejado.
        """;

    public static bool TryParse(string[] argumentos, [NotNullWhen(true)] out ComandoContaLocal? comando)
    {
        comando = null;
        if (argumentos.Length is not (3 or 4) || argumentos[0] != "contas") return false;
        var email = argumentos[2].Trim();
        if (email.Length > 256 || !MailAddress.TryCreate(email, out var endereco) || endereco.Address != email)
            return false;
        comando = argumentos[1] switch
        {
            "definir-perfil" when argumentos.Length == 4
                && argumentos[3] is PerfisNegocio.Pecuarista or PerfisNegocio.Cooperativa => new DefinirPerfilLocal(email, argumentos[3]),
            "conceder-administrador" when argumentos.Length == 3 => new ConcederAdministradorLocal(email),
            _ => null
        };
        return comando is not null;
    }
}

public sealed record DefinirPerfilLocal(string Email, string Perfil) : ComandoContaLocal(Email);
public sealed record ConcederAdministradorLocal(string Email) : ComandoContaLocal(Email);
public sealed record ResultadoComandoLocal(int CodigoSaida, string Mensagem);
