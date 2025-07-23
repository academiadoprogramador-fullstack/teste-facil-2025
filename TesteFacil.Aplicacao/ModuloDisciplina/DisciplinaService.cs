using FluentResults;
using Microsoft.Extensions.Logging;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;

namespace TesteFacil.Aplicacao;

public class DisciplinaService
{
    private readonly IRepositorioDisciplina repositorioDisciplina;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<DisciplinaService> logger;

    public DisciplinaService(
        IRepositorioDisciplina repositorioDisciplina,
        IUnitOfWork unitOfWork,
        ILogger<DisciplinaService> logger
    )
    {
        this.repositorioDisciplina = repositorioDisciplina;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public Result Cadastrar(Disciplina disciplina)
    {
        var registros = repositorioDisciplina.SelecionarRegistros();

        if (registros.Any(i => i.Nome.Equals(disciplina.Nome)))
            return Result.Fail("Já existe uma disciplina registrada com este nome.");

        try
        {
            repositorioDisciplina.Cadastrar(disciplina);

            unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro de {@ViewModel}.",
                disciplina
            );
        }

        return Result.Ok();
    }

    public Result Editar(Guid id, Disciplina disciplinaEditada)
    {
        var registros = repositorioDisciplina.SelecionarRegistros();

        if (registros.Any(i => !i.Id.Equals(id) && i.Nome.Equals(disciplinaEditada.Nome)))
            return Result.Fail("Já existe uma disciplina registrada com este nome.");

        try
        {
            repositorioDisciplina.Editar(id, disciplinaEditada);

            unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante a edição do registro {@ViewModel}.",
                disciplinaEditada
            );
        }

        return Result.Ok();
    }

    public Result Excluir(Guid id)
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

        return Result.Ok();
    }

    public Result<Disciplina> SelecionarPorId(Guid id)
    {
        var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return Result.Fail("Não foi possível encontrar o registro.");

        return registroSelecionado;
    }

    public Result<List<Disciplina>> SelecionarTodos()
    {
        var registros = new List<Disciplina>();

        try
        {
            registros = repositorioDisciplina.SelecionarRegistros();

            return Result.Ok(registros);
        }
        catch (Exception ex)
        {

            logger.LogError(
                ex,
                "Ocorreu um erro durante a seleção de registros."
            );

            return Result.Fail("Ocorreu um erro inesperado ao tentar obter os registros.");
        }
    }
}
