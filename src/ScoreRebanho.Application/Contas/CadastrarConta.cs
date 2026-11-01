using System.Net.Mail;
using FluentResults;

namespace ScoreRebanho.Application.Contas;

public sealed class CadastrarConta(ICadastroContas cadastro)
{
    public async Task<Result<ContaCriadaDto>> ExecutarAsync(CadastrarContaDto dados, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dados);
        var email = dados.Email?.Trim() ?? "";
        if (email.Length > 256 || !MailAddress.TryCreate(email, out var endereco) || endereco.Address != email)
            return Result.Fail<ContaCriadaDto>(new Error("Informe um e-mail válido.").WithMetadata("Campo", "Email"));
        if (string.IsNullOrWhiteSpace(dados.Senha) || dados.Senha.Length > 128)
            return Result.Fail<ContaCriadaDto>(new Error("Informe uma senha com até 128 caracteres.").WithMetadata("Campo", "Senha"));

        var resultado = await cadastro.CadastrarAsync(new CadastrarContaDto { Email = email, Senha = dados.Senha }, cancellationToken);
        if (resultado.Falhas.Count > 0)
            return Result.Fail<ContaCriadaDto>(resultado.Falhas.Select(f => new Error(f.Mensagem).WithMetadata("Campo", f.Campo)));
        if (resultado.UsuarioId is null)
            throw new InvalidOperationException("O cadastro não retornou um identificador de usuário.");
        return Result.Ok(new ContaCriadaDto(resultado.UsuarioId));
    }
}

