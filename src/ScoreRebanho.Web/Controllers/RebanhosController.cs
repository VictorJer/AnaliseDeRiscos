using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Relatorios;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Web.ViewModels.Rebanhos;
using ScoreRebanho.Web.ViewModels.Relatorios;

namespace ScoreRebanho.Web.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class RebanhosController(IMapper mapper, CriarRebanho criar, ListarRebanhos listar,
    ConsultarRebanho consultar, ConsultarEdicaoRebanho edicao, AtualizarRebanho atualizar,
    ExcluirRebanho excluir, ConferirOcupacao conferir) : ControllerComFalhas
{
    [HttpGet]
    public async Task<IActionResult> Index(PaginacaoViewModel consulta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest("Informe uma página válida.");
        var resultado = await listar.ExecutarAsync(mapper.Map<ListarRebanhosDto>(consulta), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível listar os rebanhos.");
        return View(mapper.Map<ListaRebanhosViewModel>(resultado.Value));
    }

    [HttpGet]
    public IActionResult Cadastrar() => View(new RebanhoFormularioViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar(RebanhoFormularioViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var resultado = await criar.ExecutarAsync(mapper.Map<CriarRebanhoDto>(model), cancellationToken);
        if (resultado.IsSuccess) return RedirectToAction(nameof(Detalhes), new { id = resultado.Value.Id });
        var acesso = TratarAcesso(resultado.Errors);
        if (acesso is not null) return acesso;
        AdicionarErros(resultado.Errors);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await consultar.ExecutarAsync(new(id), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível consultar o rebanho.");
        return resultado.Value.Relatorio switch
        {
            ResumoPecuaristaDto => View("~/Views/Relatorios/Pecuarista.cshtml", mapper.Map<RelatorioPecuaristaViewModel>(resultado.Value)),
            RelatorioCooperativaDto => View("~/Views/Relatorios/Cooperativa.cshtml", mapper.Map<RelatorioCooperativaViewModel>(resultado.Value)),
            _ => throw new InvalidOperationException("Representação de relatório não suportada.")
        };
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await edicao.ExecutarAsync(new(id), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível abrir a edição.");
        return View(mapper.Map<RebanhoFormularioViewModel>(resultado.Value));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, RebanhoFormularioViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return NotFound();
        var original = await edicao.ExecutarAsync(new(id), cancellationToken);
        if (original.IsFailed)
            return TratarAcesso(original.Errors) ?? BadRequest("Não foi possível abrir a edição.");
        if (!ModelState.IsValid) return View(model);
        var resultado = await atualizar.ExecutarAsync(mapper.Map<AtualizarRebanhoDto>(model), cancellationToken);
        if (resultado.IsSuccess) return RedirectToAction(nameof(Detalhes), new { id });
        var acesso = TratarAcesso(resultado.Errors);
        if (acesso is not null) return acesso;
        AdicionarErros(resultado.Errors);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> ConferirCadastro(RebanhoFormularioViewModel model, CancellationToken cancellationToken) =>
        ConferirFormulario(model, "Cadastrar", cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConferirEdicao(Guid id, RebanhoFormularioViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return NotFound();
        var original = await edicao.ExecutarAsync(new(id), cancellationToken);
        if (original.IsFailed)
            return TratarAcesso(original.Errors) ?? BadRequest("Não foi possível conferir o rebanho.");
        return await ConferirFormulario(model, "Editar", cancellationToken);
    }

    private async Task<IActionResult> ConferirFormulario(RebanhoFormularioViewModel model, string tela, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(tela, model);
        var resultado = await conferir.ExecutarAsync(mapper.Map<EntradaSimulacaoDto>(model.Entradas), cancellationToken);
        if (resultado.IsFailed)
        {
            var acesso = TratarAcesso(resultado.Errors);
            if (acesso is not null) return acesso;
            AdicionarErros(resultado.Errors);
        }
        else model.Conferencia = mapper.Map<ConferenciaOcupacaoViewModel>(resultado.Value);
        return View(tela, model);
    }

    [HttpGet]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await edicao.ExecutarAsync(new(id), cancellationToken);
        if (resultado.IsFailed)
            return TratarAcesso(resultado.Errors) ?? BadRequest("Não foi possível abrir a confirmação.");
        return View(mapper.Map<ConfirmacaoExclusaoViewModel>(resultado.Value));
    }

    [HttpPost, ActionName("Excluir"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConfirmado(Guid id, ExcluirRebanhoViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return NotFound();
        var original = await edicao.ExecutarAsync(new(id), cancellationToken);
        if (original.IsFailed)
            return TratarAcesso(original.Errors) ?? BadRequest("Não foi possível confirmar a exclusão.");
        if (!ModelState.IsValid) return BadRequest("Dados da confirmação inválidos.");
        var resultado = await excluir.ExecutarAsync(mapper.Map<ExcluirRebanhoDto>(model), cancellationToken);
        if (resultado.IsSuccess) return RedirectToAction(nameof(Index));
        var acesso = TratarAcesso(resultado.Errors);
        if (acesso is not null) return acesso;
        AdicionarErros(resultado.Errors);
        return View("Excluir", mapper.Map<ConfirmacaoExclusaoViewModel>(original.Value));
    }
}
