using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application.Administracao;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Web.ViewModels.Administracao;
using ScoreRebanho.Web.ViewModels.Rebanhos;

namespace ScoreRebanho.Web.Controllers;

[Authorize(Roles = PapeisContas.Administrador)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AdministracaoScoresController(IMapper mapper, ListarScoresAdministrativos listar,
    ConsultarScoreAdministrativo consultar) : ControllerComFalhas
{
    [HttpGet]
    public async Task<IActionResult> Index(PaginacaoViewModel consulta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest("Informe uma página válida.");
        var resultado = await listar.ExecutarAsync(mapper.Map<ListarScoresAdministrativosDto>(consulta), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível listar os scores.");
        return View(mapper.Map<ListaScoresAdministrativosViewModel>(resultado.Value));
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await consultar.ExecutarAsync(new(id), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível consultar o score.");
        return View(mapper.Map<RelatorioScoreAdministrativoViewModel>(resultado.Value));
    }
}
