using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ScoreRebanho.Application.Localizacao;
using ScoreRebanho.Web.ViewModels.Rebanhos;

namespace ScoreRebanho.Web.Controllers;

[Authorize]
[EnableRateLimiting("consultas-externas")]
public sealed class LocalizacaoController(BuscarMunicipios buscar, IMapper mapper) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Municipios(BuscaMunicipiosViewModel entrada, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { mensagem = "Informe um município com 3 a 100 caracteres." });
        var resultado = await buscar.ExecutarAsync(mapper.Map<BuscarMunicipiosDto>(entrada), cancellationToken);
        if (resultado.IsFailed)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensagem = resultado.Errors[0].Message });
        return Json(mapper.Map<List<MunicipioViewModel>>(resultado.Value));
    }
}
