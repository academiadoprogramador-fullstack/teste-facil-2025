using Microsoft.AspNetCore.Mvc;
using TesteFacil.Aplicacao.ModuloAutenticacao;
using TesteFacil.Dominio.ModuloAutenticacao;
using TesteFacil.WebApp.Models;

namespace TesteFacil.WebApp.Controllers;

[Route("autenticacao")]
public class AutenticacaoController : Controller
{
    private readonly AutenticacaoService autenticacaoService;

    public AutenticacaoController(AutenticacaoService autenticacaoService)
    {
        this.autenticacaoService = autenticacaoService;
    }

    [HttpGet("registro")]
    public IActionResult Registro()
    {
        var registroVm = new RegistroViewModel();

        return View(registroVm);
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroViewModel registroVm)
    {
        var usuario = new Usuario
        {
            UserName = registroVm.Email,
            Email = registroVm.Email,
        };

        var resultado = await autenticacaoService.RegistrarAsync(usuario, registroVm.Senha!);

        if (resultado.IsFailed)
            return RedirectToAction(nameof(Registro));

        return RedirectToAction(nameof(HomeController.Index), "Home", new { area = string.Empty });
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var loginVm = new LoginViewModel();

        return View(loginVm);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await autenticacaoService.LogoutAsync();

        return RedirectToAction(nameof(Login));
    }
}
