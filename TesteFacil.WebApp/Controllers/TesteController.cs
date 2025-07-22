using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Dominio.ModuloQuestao;
using TesteFacil.Dominio.ModuloTeste;
using TesteFacil.WebApp.Models;

namespace TesteFacil.WebApp.Controllers;

[Route("testes")]
public class TesteController : Controller
{
    private readonly IRepositorioTeste repositorioTeste;
    private readonly IRepositorioQuestao repositorioQuestao;
    private readonly IRepositorioDisciplina repositorioDisciplina;
    private readonly IRepositorioMateria repositorioMateria;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<TesteController> logger;

    public TesteController(
        IRepositorioTeste repositorioTeste,
        IRepositorioQuestao repositorioQuestao,
        IRepositorioDisciplina repositorioDisciplina,
        IRepositorioMateria repositorioMateria,
        IUnitOfWork unitOfWork,
        ILogger<TesteController> logger
    )
    {
        this.repositorioTeste = repositorioTeste;
        this.repositorioQuestao = repositorioQuestao;
        this.repositorioDisciplina = repositorioDisciplina;
        this.repositorioMateria = repositorioMateria;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var registros = repositorioTeste.SelecionarRegistros();

        var visualizarVm = new VisualizarTestesViewModel(registros);
        
        return View(visualizarVm);
    }

    [HttpGet("gerar/primeira-etapa")]
    public IActionResult PrimeiraEtapaGerar()
    {
        var disciplinas = repositorioDisciplina.SelecionarRegistros();

        var primeiraEtapaVm = new PrimeiraEtapaGerarTesteViewModel
        {
            DisciplinasDisponiveis = disciplinas
                .Select(d => new SelectListItem(d.Nome, d.Id.ToString()))
                .ToList()
        };

        return View(primeiraEtapaVm);
    }

    [HttpPost("gerar/primeira-etapa")]
    public IActionResult PrimeiraEtapaGerar(PrimeiraEtapaGerarTesteViewModel primeiraEtapaVm)
    {
        var disciplinaSelecionada = repositorioDisciplina.SelecionarRegistroPorId(primeiraEtapaVm.DisciplinaId);

        if (disciplinaSelecionada is null)
            return RedirectToAction(nameof(Index));

        var materias = repositorioMateria
            .SelecionarRegistros()
            .Where(m => m.Disciplina.Equals(disciplinaSelecionada))
            .Where(m => m.Serie.Equals(primeiraEtapaVm.Serie))
            .ToList();

        var segundaEtapaVm = PrimeiraEtapaGerarTesteViewModel.AvancarEtapa(
            primeiraEtapaVm,
            disciplinaSelecionada,
            materias
        );

        var jsonString = JsonSerializer.Serialize(segundaEtapaVm);

        TempData.Add(nameof(SegundaEtapaGerarTesteViewModel), jsonString);

        return RedirectToAction(nameof(SegundaEtapaGerar));
    }

    [HttpGet("gerar/segunda-etapa")]
    public IActionResult SegundaEtapaGerar()
    {
        var conseguiuRecuperar = TempData
            .TryGetValue(nameof(SegundaEtapaGerarTesteViewModel), out var value);

        if (!conseguiuRecuperar || value is not string jsonString)
            return RedirectToAction(nameof(Index));

        var segundaEtapaVm = JsonSerializer.Deserialize<SegundaEtapaGerarTesteViewModel>(jsonString);

        return View(segundaEtapaVm);
    }

    [HttpPost("gerar/segunda-etapa/sortear-questoes")]
    public IActionResult SortearQuestoes(SegundaEtapaGerarTesteViewModel segundaEtapaVm)
    {
        List<Questao> questoesSorteadas;

        if (segundaEtapaVm.Recuperacao)
        {
            questoesSorteadas = repositorioQuestao.SelecionarQuestoesPorDisciplinaESerie(
                segundaEtapaVm.DisciplinaId,
                segundaEtapaVm.Serie,
                segundaEtapaVm.QuantidadeQuestoes
            );
        }
        else
        {
            if (!segundaEtapaVm.MateriaId.HasValue)
                throw new InvalidOperationException("Não foi possível obter o ID da matéria selecionada.");

            questoesSorteadas = repositorioQuestao.SelecionarQuestoesPorMateria(
                segundaEtapaVm.MateriaId.Value,
                segundaEtapaVm.QuantidadeQuestoes
            );

            segundaEtapaVm.MateriasDisponiveis = repositorioMateria
                .SelecionarRegistros()
                .Where(m => m.Disciplina.Id.Equals(segundaEtapaVm.DisciplinaId))
                .Where(m => m.Serie.Equals(segundaEtapaVm.Serie))
                .Select(m => new SelectListItem(m.Nome, m.Id.ToString()))
                .ToList();
        }

        segundaEtapaVm.QuestoesSorteadas = questoesSorteadas.Select(q => new DetalhesQuestaoViewModel(
            q.Id,
            q.Enunciado,
            q.Materia.Nome,
            q.UtilizadaEmTeste ? "Sim" : "Não",
            q.AlternativaCorreta?.Resposta ?? string.Empty,
            q.Alternativas
        )).ToList();

        return View(nameof(SegundaEtapaGerar), segundaEtapaVm);
    }

    [HttpPost("gerar/confirmar")]
    public IActionResult ConfirmarGeracao(SegundaEtapaGerarTesteViewModel segundaEtapaVm)
    {
        return View();
    }
}
