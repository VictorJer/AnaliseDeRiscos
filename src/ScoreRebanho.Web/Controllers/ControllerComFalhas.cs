using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace ScoreRebanho.Web.Controllers;

public abstract class ControllerComFalhas : Controller
{
    protected IActionResult? TratarAcesso(IEnumerable<IError> erros)
    {
        var codigos = erros.Select(e => e.Metadata.GetValueOrDefault("Codigo") as string).ToArray();
        if (codigos.Contains("NAO_AUTENTICADO")) return Challenge();
        if (codigos.Contains("PERFIL_INVALIDO")) return Forbid();
        if (codigos.Contains("NAO_ENCONTRADO")) return NotFound();
        if (codigos.Contains("CONFLITO")) Response.StatusCode = StatusCodes.Status409Conflict;
        return null;
    }

    protected void AdicionarErros(IEnumerable<IError> erros)
    {
        foreach (var erro in erros)
        {
            var campo = erro.Metadata.GetValueOrDefault("Campo") as string ?? "";
            if (campo.Length > 0 && campo is not ("Nome" or "Propriedade" or "Pagina" or "Entradas"))
                campo = "Entradas." + campo;
            ModelState.AddModelError(campo, erro.Message);
        }
    }
}
