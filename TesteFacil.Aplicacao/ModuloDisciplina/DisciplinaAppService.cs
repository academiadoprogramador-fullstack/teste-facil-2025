using FluentResults;
using Microsoft.Extensions.Logging;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;

namespace TesteFacil.Aplicacao.ModuloDisciplina;

public class DisciplinaAppService
{
    private readonly IRepositorioDisciplina repositorioDisciplina;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<DisciplinaAppService> logger;

    public DisciplinaAppService(
        IRepositorioDisciplina repositorioDisciplina,
        IUnitOfWork unitOfWork,
        ILogger<DisciplinaAppService> logger
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

            return Result.Ok();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro de {@Registro}.",
                disciplina
            );

            return Result.Fail("Ocorreu um erro inesperado ao tentar cadastrar o registro.");
        }
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

            return Result.Ok();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante a edição do registro {@Registro}.",
                disciplinaEditada
            );

            return Result.Fail("Ocorreu um erro inesperado ao tentar editar o registro.");
        }
    }

    public Result Excluir(Guid id)
    {
        try
        {
            repositorioDisciplina.Excluir(id);

            unitOfWork.Commit();

            return Result.Ok();

        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante a exclusão do registro {Id}.",
                id
            );

            return Result.Fail("Ocorreu um erro inesperado ao tentar excluir o registro.");
        }
    }

    public Result<Disciplina> SelecionarPorId(Guid id)
    {
        try
        {
            var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(id);

            if (registroSelecionado is null)
                return Result.Fail("Não foi possível obter o registro.");

            return Result.Ok(registroSelecionado);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a seleção do registro {Id}.",
                id
            );

            return Result.Fail("Ocorreu um erro inesperado ao tentar obter o registro.");
        }
    }

    public Result<List<Disciplina>> SelecionarTodos()
    {
        try
        {
            var registros = repositorioDisciplina.SelecionarRegistros();

            return Result.Ok(registros);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a seleção de registros."
            );

            return Result.Fail("Ocorreu um erro inesperado ao tentar obter o registros.");
        }
    }
}
