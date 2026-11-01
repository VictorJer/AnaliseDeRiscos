using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Web.ViewModels.Rebanhos;

namespace ScoreRebanho.Web.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SimulacoesController(IMapper mapper, GerarSimulacao gerar,
    ConsultarEdicaoSimulacao edicao, ConferirOcupacao conferir) : ControllerComFalhas
{
    [HttpGet]
    public IActionResult Nova() => View(new SimulacaoFormularioViewModel());

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await edicao.ExecutarAsync(new(id), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível abrir a simulação.");
        return View("Nova", mapper.Map<SimulacaoFormularioViewModel>(resultado.Value));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Gerar(SimulacaoFormularioViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Nova", model);
        var resultado = await gerar.ExecutarAsync(mapper.Map<GerarSimulacaoDto>(model), cancellationToken);
        if (resultado.IsSuccess)
            return RedirectToAction("Detalhes", "Historico", new { id = resultado.Value.Id });
        var acesso = TratarAcesso(resultado.Errors);
        if (acesso is not null) return acesso;
        AdicionarErros(resultado.Errors);
        return View("Nova", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Conferir(SimulacaoFormularioViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Nova", model);
        var resultado = await conferir.ExecutarAsync(mapper.Map<EntradaSimulacaoDto>(model.Entradas), cancellationToken);
        if (resultado.IsFailed)
        {
            var acesso = TratarAcesso(resultado.Errors);
            if (acesso is not null) return acesso;
            AdicionarErros(resultado.Errors);
        }
        else model.Conferencia = mapper.Map<ConferenciaOcupacaoViewModel>(resultado.Value);
        return View("Nova", model);
    }
}
