using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using TesteFacil.Aplicacao.ModuloDisciplina;
using TesteFacil.Aplicacao.ModuloMateria;
using TesteFacil.WebApp.Models;

namespace TesteFacil.WebApp.Controllers;

[Route("materias")]
public class MateriaController : Controller
{
    private readonly MateriaAppService materiaAppService;
    private readonly DisciplinaService disciplinaAppService;

    public MateriaController(
        MateriaAppService materiaAppService,
        DisciplinaService disciplinaAppService
    )
    {
        this.materiaAppService = materiaAppService;
        this.disciplinaAppService = disciplinaAppService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var resultado = materiaAppService.SelecionarTodos();

        if (resultado.IsFailed)
        {
            var notificacaoJson = NotificacaoViewModel.GerarNotificacaoSerializada(
                "Erro ao selecionar registros",
                resultado.Errors[0].Message
            );

            TempData.Add(nameof(NotificacaoViewModel), notificacaoJson);

            return RedirectToAction(nameof(Index));
        }

        var registros = resultado.Value;

        var visualizarVM = new VisualizarMateriasViewModel(registros);

        return View(visualizarVM);
    }

    [HttpGet("cadastrar")]
    public IActionResult Cadastrar()
    {
        var resultadoDisciplinas = disciplinaAppService.SelecionarTodos();

        var disciplinas = resultadoDisciplinas.Value;

        var cadastrarVM = new CadastrarMateriaViewModel(disciplinas);

        return View(cadastrarVM);
    }

    [HttpPost("cadastrar")]
    [ValidateAntiForgeryToken]
    public IActionResult Cadastrar(CadastrarMateriaViewModel cadastrarVM)
    {
        var resultadoDisciplinas = disciplinaAppService.SelecionarTodos();

        var disciplinas = resultadoDisciplinas.Value;

        var entidade = FormularioMateriaViewModel.ParaEntidade(cadastrarVM, disciplinas);

        var resultado = materiaAppService.Cadastrar(entidade);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError("CadastroUnico", resultado.Errors[0].Message);

            cadastrarVM.DisciplinasDisponiveis = disciplinas
                .Select(d => new SelectListItem(d.Nome, d.Id.ToString()))
                .ToList();

            return View(cadastrarVM);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id:guid}")]
    public ActionResult Editar(Guid id)
    {
        var resultadoDisciplinas = disciplinaAppService.SelecionarTodos();

        var disciplinas = resultadoDisciplinas.Value;

        var resultado = materiaAppService.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            var notificacaoJson = NotificacaoViewModel.GerarNotificacaoSerializada(
                "Erro ao selecionar registro",
                resultado.Errors[0].Message
            );

            TempData.Add(nameof(NotificacaoViewModel), notificacaoJson);

            return RedirectToAction(nameof(Index));
        }

        var registroSelecionado = resultado.Value;

        var editarVM = new EditarMateriaViewModel(
            id,
            registroSelecionado.Nome,
            registroSelecionado.Serie,
            registroSelecionado.Disciplina.Id,
            disciplinas
        );

        return View(editarVM);
    }

    [HttpPost("editar/{id:guid}")]
    [ValidateAntiForgeryToken]
    public ActionResult Editar(Guid id, EditarMateriaViewModel editarVM)
    {
        var resultadoDisciplinas = disciplinaAppService.SelecionarTodos();

        var disciplinas = resultadoDisciplinas.Value;

        var entidadeEditada = FormularioMateriaViewModel.ParaEntidade(editarVM, disciplinas);

        var resultado = materiaAppService.Editar(id, entidadeEditada);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError("CadastroUnico", resultado.Errors[0].Message);

            editarVM.DisciplinasDisponiveis = disciplinas
                .Select(d => new SelectListItem(d.Nome, d.Id.ToString()))
                .ToList();

            return View(editarVM);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("excluir/{id:guid}")]
    public IActionResult Excluir(Guid id)
    {
        var resultado = materiaAppService.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            var notificacaoJson = NotificacaoViewModel.GerarNotificacaoSerializada(
                "Erro ao selecionar registro",
                resultado.Errors[0].Message
            );

            TempData.Add(nameof(NotificacaoViewModel), notificacaoJson);

            return RedirectToAction(nameof(Index));
        }

        var registroSelecionado = resultado.Value;

        var excluirVM = new ExcluirMateriaViewModel(
            registroSelecionado.Id,
            registroSelecionado.Nome
        );

        return View(excluirVM);
    }

    [HttpPost("excluir/{id:guid}")]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirConfirmado(Guid id)
    {
        var resultado = materiaAppService.Excluir(id);

        if (resultado.IsFailed)
        {
            var notificacaoJson = NotificacaoViewModel.GerarNotificacaoSerializada(
                "Erro ao selecionar registro",
                resultado.Errors[0].Message
            );

            TempData.Add(nameof(NotificacaoViewModel), notificacaoJson);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("detalhes/{id:guid}")]
    public IActionResult Detalhes(Guid id)
    {
        var resultado = materiaAppService.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            var notificacaoJson = NotificacaoViewModel.GerarNotificacaoSerializada(
                "Erro ao selecionar registro",
                resultado.Errors[0].Message
            );

            TempData.Add(nameof(NotificacaoViewModel), notificacaoJson);

            return RedirectToAction(nameof(Index));
        }

        var detalhesVm = DetalhesMateriaViewModel.ParaDetalhesVm(resultado.Value);

        return View(detalhesVm);
    }
}
