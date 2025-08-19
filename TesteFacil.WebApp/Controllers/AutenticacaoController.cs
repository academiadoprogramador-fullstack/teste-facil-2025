using Microsoft.AspNetCore.Mvc;
using TesteFacil.Aplicacao.ModuloAutenticacao;
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

    [HttpGet("login")]
    public IActionResult Login()
    {
        var loginVm = new LoginViewModel();

        return View(loginVm);
    }
}
