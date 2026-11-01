using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Web.ViewModels.Conta;

namespace ScoreRebanho.Web.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ContaController(
    CadastrarConta cadastrarConta, IMapper mapper,
    UserManager<IdentityUser> usuarios, SignInManager<IdentityUser> sessoes) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Cadastrar() =>
        User.Identity?.IsAuthenticated == true ? RedirectToAction(nameof(HomeController.Index), "Home") : View(new CadastrarViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar(CadastrarViewModel model, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(HomeController.Index), "Home");
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await cadastrarConta.ExecutarAsync(mapper.Map<CadastrarContaDto>(model), cancellationToken);
        if (resultado.IsFailed)
        {
            foreach (var erro in resultado.Errors)
            {
                var campo = erro.Metadata.TryGetValue("Campo", out var valor) ? valor as string ?? "" : "";
                ModelState.AddModelError(campo, erro.Message);
            }
            return View(model);
        }

        var usuario = await usuarios.FindByIdAsync(resultado.Value.Id)
            ?? throw new InvalidOperationException("Conta recém-criada não encontrada.");
        await sessoes.SignInAsync(usuario, isPersistent: false);
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Entrar(string? returnUrl = null) =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction(nameof(HomeController.Index), "Home")
            : View(new EntrarViewModel { ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null });

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(EntrarViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(HomeController.Index), "Home");
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await sessoes.PasswordSignInAsync(model.Email.Trim(), model.Senha,
            model.ManterConectado, lockoutOnFailure: true);
        if (resultado.Succeeded)
            return Url.IsLocalUrl(model.ReturnUrl)
                ? LocalRedirect(model.ReturnUrl!)
                : RedirectToAction(nameof(HomeController.Index), "Home");

        ModelState.AddModelError("", resultado.IsLockedOut
            ? "Acesso temporariamente bloqueado por tentativas sem sucesso. Tente novamente em alguns minutos."
            : "E-mail ou senha inválidos.");
        return View(model);
    }

    [Authorize, HttpGet]
    public IActionResult AlterarSenha() => View(new AlterarSenhaViewModel());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await usuarios.GetUserAsync(User);
        if (usuario is null)
        {
            await sessoes.SignOutAsync();
            return RedirectToAction(nameof(Entrar));
        }

        var resultado = await usuarios.ChangePasswordAsync(usuario, model.SenhaAtual, model.NovaSenha);
        if (!resultado.Succeeded)
        {
            foreach (var erro in resultado.Errors)
            {
                var campo = erro.Code == nameof(IdentityErrorDescriber.PasswordMismatch)
                    ? nameof(model.SenhaAtual)
                    : erro.Code.StartsWith("Password", StringComparison.Ordinal) ? nameof(model.NovaSenha) : "";
                ModelState.AddModelError(campo, erro.Description);
            }
            return View(model);
        }

        await sessoes.RefreshSignInAsync(usuario);
        TempData["SenhaAlterada"] = true;
        return RedirectToAction(nameof(AlterarSenha));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await sessoes.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult AcessoNegado() => StatusCode(StatusCodes.Status403Forbidden);
}
