using FluentResults;
using ScoreRebanho.Domain.Rebanhos;

namespace ScoreRebanho.Application.Rebanhos;

internal static class FalhasRebanhos
{
    internal static Error NaoEncontrado() => new Error("Rebanho não encontrado.").WithMetadata("Codigo", "NAO_ENCONTRADO");
    internal static Error Conflito() => new Error("O rebanho foi alterado em outra sessão. Abra os dados atualizados antes de tentar novamente.").WithMetadata("Codigo", "CONFLITO");
    internal static Error Entrada(string campo, string mensagem) =>
        new Error(mensagem).WithMetadata("Codigo", "ENTRADA_INVALIDA").WithMetadata("Campo", campo);

    internal static Result<IdentificacaoRebanho> Identificacao(string nome, string propriedade)
    {
        try
        {
            return Result.Ok(new IdentificacaoRebanho(nome, propriedade));
        }
        catch (ArgumentException ex) when (ex.ParamName is "nome" or "propriedade")
        {
            return ex.ParamName == "nome"
                ? Result.Fail<IdentificacaoRebanho>(Entrada("Nome", $"Informe um nome com até {IdentificacaoRebanho.TamanhoMaximoNome} caracteres."))
                : Result.Fail<IdentificacaoRebanho>(Entrada("Propriedade", $"Informe uma propriedade com até {IdentificacaoRebanho.TamanhoMaximoPropriedade} caracteres."));
        }
    }

    internal static Result<RebanhoGravadoDto> Gravacao(ResultadoGravacaoRebanho resultado) => resultado.Estado switch
    {
        EstadoGravacaoRebanho.Sucesso when resultado.Rebanho is not null => Result.Ok(resultado.Rebanho),
        EstadoGravacaoRebanho.NaoEncontrado => Result.Fail<RebanhoGravadoDto>(NaoEncontrado()),
        EstadoGravacaoRebanho.Conflito => Result.Fail<RebanhoGravadoDto>(Conflito()),
        _ => throw new InvalidOperationException("Estado de gravação inválido retornado pela persistência.")
    };

    internal static Result Exclusao(EstadoGravacaoRebanho estado) => estado switch
    {
        EstadoGravacaoRebanho.Sucesso => Result.Ok(),
        EstadoGravacaoRebanho.NaoEncontrado => Result.Fail(NaoEncontrado()),
        EstadoGravacaoRebanho.Conflito => Result.Fail(Conflito()),
        _ => throw new InvalidOperationException("Estado de exclusão inválido retornado pela persistência.")
    };
}
