using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ScoreRebanho.Application.Clima;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Web.ViewModels.Acompanhamento;
using ScoreRebanho.Web.ViewModels.Rebanhos;

namespace ScoreRebanho.Web.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AcompanhamentoController(ListarRebanhos listar, ConsultarPrevisaoRebanho consultar, IMapper mapper) : ControllerComFalhas
{
    [HttpGet]
    public async Task<IActionResult> Index(PaginacaoViewModel entrada, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest("Informe uma página válida.");
        var resultado = await listar.ExecutarAsync(mapper.Map<ListarRebanhosDto>(entrada), cancellationToken);
        if (resultado.IsFailed) return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível listar os rebanhos.");
        return View(mapper.Map<PaginaAcompanhamentoViewModel>(resultado.Value));
    }

    [HttpGet]
    [EnableRateLimiting("consultas-externas")]
    public async Task<IActionResult> Detalhes([FromRoute] ConsultaPrevisaoViewModel entrada, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();
        var resultado = await consultar.ExecutarAsync(mapper.Map<ConsultarPrevisaoDto>(entrada), cancellationToken);
        if (resultado.IsSuccess) return View(mapper.Map<PrevisaoRebanhoViewModel>(resultado.Value));
        var acesso = TratarAcesso(resultado.Errors);
        if (acesso is not null) return acesso;
        var semLocalizacao = resultado.Errors.Any(e => e.Metadata.GetValueOrDefault("Codigo") as string == "SEM_LOCALIZACAO");
        if (!semLocalizacao) Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View("Indisponivel", new FalhaPrevisaoViewModel
        {
            Id = entrada.Id, Mensagem = resultado.Errors[0].Message, SemLocalizacao = semLocalizacao
        });
    }
}
