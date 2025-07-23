using Microsoft.AspNetCore.Mvc;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.WebApp.Models;
using TesteFacil.WebApp.Services;

namespace TesteFacil.WebApp.Controllers;

[Route("disciplinas")]
public class DisciplinaController : Controller
{
    private readonly DisciplinaService disciplinaService;
    private readonly IRepositorioDisciplina repositorioDisciplina;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<DisciplinaController> logger;

    public DisciplinaController(
        DisciplinaService disciplinaService,
        IRepositorioDisciplina repositorioDisciplina,
        IUnitOfWork unitOfWork,
        ILogger<DisciplinaController> logger
    )
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
        this.disciplinaService = disciplinaService;
        this.repositorioDisciplina = repositorioDisciplina;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var registros = repositorioDisciplina.SelecionarRegistros();

        var visualizarVM = new VisualizarDisciplinasViewModel(registros);

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

        var resultado = disciplinaService.Cadastrar(entidade);

        if (resultado.IsFailed)
        {
            foreach (var erro in resultado.Errors)
            {
                if (erro.Metadata["ErrorType"].ToString() == "BadRequest")
                {
                    ModelState.AddModelError("BadRequest", erro.Reasons.First().Message);
                    break;
                }
            }

            return View(cadastrarVM);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id:guid}")]
    public ActionResult Editar(Guid id)
    {
        var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return RedirectToAction(nameof(Index));

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

        var resultado = disciplinaService.Editar(id, entidadeEditada);

        if (resultado.IsFailed)
        {
            foreach (var erro in resultado.Errors)
            {
                if (erro.Metadata["ErrorType"].ToString() == "BadRequest")
                {
                    ModelState.AddModelError("BadRequest", erro.Reasons.First().Message);
                    break;
                }
            }

            return View(editarVM);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("excluir/{id:guid}")]
    public IActionResult Excluir(Guid id)
    {
        var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return RedirectToAction(nameof(Index));

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
        try
        {
            repositorioDisciplina.Excluir(id);

            unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante a exclusão do registro {Id}.",
                id
            );
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpGet("detalhes/{id:guid}")]
    public IActionResult Detalhes(Guid id)
    {
        var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return RedirectToAction(nameof(Index));

        var detalhesVM = new DetalhesDisciplinaViewModel(
            id,
            registroSelecionado.Nome,
            registroSelecionado.Materias
        );

        return View(detalhesVM);
    }
}
