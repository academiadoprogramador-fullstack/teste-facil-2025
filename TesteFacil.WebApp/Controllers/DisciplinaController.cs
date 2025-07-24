using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TesteFacil.Aplicacao.ModuloDisciplina;
using TesteFacil.WebApp.Models;

namespace TesteFacil.WebApp.Controllers;

[Route("disciplinas")]
public class DisciplinaController : Controller
{
    private readonly DisciplinaAppService disciplinaAppService;

    public DisciplinaController(DisciplinaAppService disciplinaAppService)
    {
        this.disciplinaAppService = disciplinaAppService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var resultado = disciplinaAppService.SelecionarTodos();

        if (resultado.IsFailed)
            return RedirectToAction("Home/Index");

        var registros = resultado.Value;

        var visualizarVM = new VisualizarDisciplinasViewModel(registros);

        var existeNotificacao = TempData.TryGetValue(nameof(NotificacaoViewModel), out var valor);

        if (existeNotificacao && valor is string jsonString)
        {
            var notificacaoVm = JsonSerializer.Deserialize<NotificacaoViewModel>(jsonString);

            ViewData.Add(nameof(NotificacaoViewModel), notificacaoVm);
        }

        return View(visualizarVM);
    }

    [HttpGet("cadastrar")]
    public IActionResult Cadastrar()
    {
        var cadastrarVM = new CadastrarDisciplinaViewModel();

        return View(cadastrarVM);
    }

    [HttpPost("cadastrar")]
    [ValidateAntiForgeryToken]
    public IActionResult Cadastrar(CadastrarDisciplinaViewModel cadastrarVM)
    {
        var entidade = FormularioDisciplinaViewModel.ParaEntidade(cadastrarVM);

        var resultado = disciplinaAppService.Cadastrar(entidade);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError("CadastroUnico", resultado.Errors[0].Message);

            return View(cadastrarVM);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id:guid}")]
    public ActionResult Editar(Guid id)
    {
        var resultado = disciplinaAppService.SelecionarPorId(id);

        if (resultado.IsFailed)
            return RedirectToAction(nameof(Index));

        var registroSelecionado = resultado.Value;

        var editarVM = new EditarDisciplinaViewModel(
            id,
            registroSelecionado.Nome
        );

        return View(editarVM);
    }

    [HttpPost("editar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public ActionResult Editar(Guid id, EditarDisciplinaViewModel editarVM)
    {
        var entidadeEditada = FormularioDisciplinaViewModel.ParaEntidade(editarVM);

        var resultado = disciplinaAppService.Editar(id, entidadeEditada);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError("CadastroUnico", resultado.Errors[0].Message);

            return View(editarVM);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("excluir/{id:guid}")]
    public IActionResult Excluir(Guid id)
    {
        var resultado = disciplinaAppService.SelecionarPorId(id);

        if (resultado.IsFailed)
            return RedirectToAction(nameof(Index));

        var registroSelecionado = resultado.Value;

        var excluirVM = new ExcluirDisciplinaViewModel(
            registroSelecionado.Id,
            registroSelecionado.Nome
        );

        return View(excluirVM);
    }

    [HttpPost("excluir/{id:guid}")]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirConfirmado(Guid id)
    {
        var resultado = disciplinaAppService.Excluir(id);

        if (resultado.IsFailed)
        {
            var notificacaoJson = NotificacaoViewModel.GerarNotificacaoSerializada(
                "Erro ao excluir registro",
                resultado.Errors[0].Message
            );

            TempData.Add(nameof(NotificacaoViewModel), notificacaoJson);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("detalhes/{id:guid}")]
    public IActionResult Detalhes(Guid id)
    {
        var resultado = disciplinaAppService.SelecionarPorId(id);
        
        if (resultado.IsFailed)
            return RedirectToAction(nameof(Index));

        var detalhesVm = DetalhesDisciplinaViewModel.ParaDetalhesVm(resultado.Value);

        return View(detalhesVm);
    }
}
