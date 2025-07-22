using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.WebApp.Models;

namespace TesteFacil.WebApp.Controllers;

[Route("materias")]
public class MateriaController : Controller
{
    private readonly IRepositorioMateria repositorioMateria;
    private readonly IRepositorioDisciplina repositorioDisciplina;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<MateriaController> logger;

    public MateriaController(
        IRepositorioMateria repositorioMateria,
        IRepositorioDisciplina repositorioDisciplina,
        IUnitOfWork unitOfWork,
        ILogger<MateriaController> logger
    )
    {
        this.repositorioMateria = repositorioMateria;
        this.repositorioDisciplina = repositorioDisciplina;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var registros = repositorioMateria.SelecionarRegistros();

        var visualizarVM = new VisualizarMateriasViewModel(registros);

        return View(visualizarVM);
    }

    [HttpGet("cadastrar")]
    public IActionResult Cadastrar()
    {
        var disciplinas = repositorioDisciplina.SelecionarRegistros();

        var cadastrarVM = new CadastrarMateriaViewModel(disciplinas);

        return View(cadastrarVM);
    }

    [HttpPost("cadastrar")]
    [ValidateAntiForgeryToken]
    public IActionResult Cadastrar(CadastrarMateriaViewModel cadastrarVM)
    {
        var registros = repositorioMateria.SelecionarRegistros();

        var disciplinas = repositorioDisciplina.SelecionarRegistros();

        if (registros.Any(i => i.Nome.Equals(cadastrarVM.Nome)))
        {
            ModelState.AddModelError(
                "CadastroUnico",
                "Já existe uma disciplina registrada com este nome."
            );

            cadastrarVM.DisciplinasDisponiveis = disciplinas
                .Select(d => new SelectListItem(d.Nome, d.Id.ToString()))
                .ToList();

            return View(cadastrarVM);
        }

        try
        {
            var entidade = FormularioMateriaViewModel.ParaEntidade(cadastrarVM, disciplinas);

            repositorioMateria.Cadastrar(entidade);

            unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro de {@ViewModel}.",
                cadastrarVM
            );
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id:guid}")]
    public ActionResult Editar(Guid id)
    {
        var registroSelecionado = repositorioMateria.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return RedirectToAction(nameof(Index));

        var disciplinas = repositorioDisciplina.SelecionarRegistros();

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
        var registros = repositorioMateria.SelecionarRegistros();

        var disciplinas = repositorioDisciplina.SelecionarRegistros();

        if (registros.Any(i => !i.Id.Equals(editarVM.Id) && i.Nome.Equals(editarVM.Nome)))
        {
            ModelState.AddModelError(
                "CadastroUnico",
                "Já existe uma disciplina registrada com este nome."
            );

            editarVM.DisciplinasDisponiveis = disciplinas
                .Select(d => new SelectListItem(d.Nome, d.Id.ToString()))
                .ToList();

            return View(editarVM);
        }

        try
        {
            var entidadeEditada = FormularioMateriaViewModel.ParaEntidade(editarVM, disciplinas);

            repositorioMateria.Editar(id, entidadeEditada);

            unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante a edição do registro {@ViewModel}.",
                editarVM
            );
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("excluir/{id:guid}")]
    public IActionResult Excluir(Guid id)
    {
        var registroSelecionado = repositorioMateria.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return RedirectToAction(nameof(Index));

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
        try
        {
            repositorioMateria.Excluir(id);

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
        var registroSelecionado = repositorioMateria.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return RedirectToAction(nameof(Index));

        var detalhesVM = new DetalhesMateriaViewModel(
            id,
            registroSelecionado.Nome,
            registroSelecionado.Serie,
            registroSelecionado.Disciplina.Nome
        );

        return View(detalhesVM);
    }
}
