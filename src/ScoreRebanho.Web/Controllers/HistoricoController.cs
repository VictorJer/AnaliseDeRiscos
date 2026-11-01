using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application.Historico;
using ScoreRebanho.Application.Relatorios;
using ScoreRebanho.Web.ViewModels.Relatorios;

namespace ScoreRebanho.Web.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class HistoricoController(ConsultarHistoricoProprio consultar, IMapper mapper) : ControllerComFalhas
{
    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await consultar.ExecutarAsync(new(id), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível consultar o histórico.");
        return resultado.Value.Relatorio switch
        {
            ResumoPecuaristaDto => View("~/Views/Relatorios/Pecuarista.cshtml", mapper.Map<RelatorioPecuaristaViewModel>(resultado.Value)),
            RelatorioCooperativaDto => View("~/Views/Relatorios/Cooperativa.cshtml", mapper.Map<RelatorioCooperativaViewModel>(resultado.Value)),
            _ => throw new InvalidOperationException("Representação de relatório não suportada.")
        };
    }
}
