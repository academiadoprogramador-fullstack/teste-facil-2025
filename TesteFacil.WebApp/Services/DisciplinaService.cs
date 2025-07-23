using FluentResults;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.WebApp.Models;

namespace TesteFacil.WebApp.Services;

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

    public Result Cadastrar(Disciplina discipina)
    {
        var registros = repositorioDisciplina.SelecionarRegistros();

        if (registros.Any(i => i.Nome.Equals(discipina.Nome)))
            return Result.Fail(ErrorResults.BadRequestError("Já existe uma disciplina registrada com este nome."));

        try
        {
            repositorioDisciplina.Cadastrar(discipina);

            unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro de {@ViewModel}.",
                discipina
            );

            throw;
        }

        return Result.Ok();
    }

    public Result Editar(Guid id, Disciplina disciplinaEditada)
    {
        var registros = repositorioDisciplina.SelecionarRegistros();

        if (registros.Any(i => !i.Id.Equals(id) && i.Nome.Equals(disciplinaEditada.Nome)))
            return Result.Fail(ErrorResults.BadRequestError("Já existe uma disciplina registrada com este nome."));

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

            throw;
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
                "Ocorreu um erro durante a edição do registro {Id}.",
                id
            );

            throw;
        }

        return Result.Ok();
    }
}
