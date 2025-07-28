using FluentResults;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using TesteFacil.Aplicacao.Compartilhado;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Dominio.ModuloQuestao;
using TesteFacil.Dominio.ModuloTeste;
using TesteFacil.Infraestrutura.Pdf;

namespace TesteFacil.Aplicacao.ModuloTeste;

public class TesteAppService
{
    private readonly IRepositorioTeste repositorioTeste;
    private readonly IRepositorioQuestao repositorioQuestao;
    private readonly IRepositorioDisciplina repositorioDisciplina;
    private readonly IRepositorioMateria repositorioMateria;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<TesteAppService> logger;

    public TesteAppService(
        IRepositorioTeste repositorioTeste,
        IRepositorioQuestao repositorioQuestao,
        IRepositorioDisciplina repositorioDisciplina,
        IRepositorioMateria repositorioMateria,
        IUnitOfWork unitOfWork,
        ILogger<TesteAppService> logger
    )
    {
        this.repositorioTeste = repositorioTeste;
        this.repositorioQuestao = repositorioQuestao;
        this.repositorioDisciplina = repositorioDisciplina;
        this.repositorioMateria = repositorioMateria;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public Result Cadastrar(Teste teste)
    {
        try
        {
            repositorioTeste.Cadastrar(teste);

            unitOfWork.Commit();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro de {@ViewModel}.",
                teste
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }

    public Result Excluir(Guid id)
    {
        try
        {
            repositorioTeste.Excluir(id);

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

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }

    public Result<Teste> SelecionarPorId(Guid id)
    {
        try
        {
            var registroSelecionado = repositorioTeste.SelecionarRegistroPorId(id);

            if (registroSelecionado is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro(id));

            return Result.Ok(registroSelecionado);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a seleção do registro {Id}.",
                id
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }

    public Result<List<Teste>> SelecionarTodos()
    {
        try
        {
            var registros = repositorioTeste.SelecionarRegistros();

            return Result.Ok(registros);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a seleção de registros."
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }

    public Result CadastrarTesteDuplicado(Teste teste)
    {
        return Cadastrar(teste);
    }

    public Result<byte[]> GerarPdf(Guid id, bool gabarito = false)
    {
        var registroSelecionado = repositorioTeste.SelecionarRegistroPorId(id);

        if (registroSelecionado is null)
            return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro(id));

        var documento = new ImpressaoTesteDocument(registroSelecionado, gabarito);

        var pdfBytes = documento.GeneratePdf();

        return pdfBytes;
    }
}
