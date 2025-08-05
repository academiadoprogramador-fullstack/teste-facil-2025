using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using TesteFacil.Aplicacao.ModuloDisciplina;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Dominio.ModuloTeste;

namespace TesteFacil.Testes.Unidade.ModuloDisciplina;


[TestClass]
[TestCategory("Testes de Unidade de Disciplina")]
public sealed class DisciplinaAppServiceTests
{
    private Mock<IRepositorioDisciplina>? repositorioDisciplinaMock;
    private Mock<IRepositorioMateria>? repositorioMateriaMock;
    private Mock<IRepositorioTeste>? repositorioTesteMock;
    private Mock<IUnitOfWork>? unitOfWorkMock;
    private Mock<ILogger<DisciplinaAppService>>? loggerMock;

    private DisciplinaAppService? appService;

    [TestInitialize]
    public void Setup()
    {
        repositorioDisciplinaMock = new Mock<IRepositorioDisciplina>();
        repositorioMateriaMock = new Mock<IRepositorioMateria>();
        repositorioTesteMock = new Mock<IRepositorioTeste>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<DisciplinaAppService>>();

        appService = new DisciplinaAppService(
            repositorioDisciplinaMock.Object,
            repositorioMateriaMock.Object,
            repositorioTesteMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public void Cadastrar_DeveRetornarOk_QuandoDisciplinaForValida()
    {
        // Arrange
        var novaDisciplina = new Disciplina("Matemática");

        repositorioDisciplinaMock?
            .Setup(r => r.SelecionarRegistros())
            .Returns(new List<Disciplina>());

        // Act
        var resultado = appService?.Cadastrar(novaDisciplina);

        // Assert
        repositorioDisciplinaMock?.Verify(r => r.Cadastrar(novaDisciplina), Times.Once);
        unitOfWorkMock?.Verify(u => u.Commit(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
    }

    [TestMethod]
    public void Cadastrar_DeveRetornarFalha_QuandoDisciplinaForDuplicada()
    {
        // Arrange
        var novaDisciplina = new Disciplina("Matemática");

        repositorioDisciplinaMock?
            .Setup(r => r.SelecionarRegistros())
            .Returns(new List<Disciplina> { new Disciplina("Matemática") });

        // Act
        var resultado = appService?.Cadastrar(novaDisciplina);

        // Assert
        repositorioDisciplinaMock?.Verify(r => r.Cadastrar(It.IsAny<Disciplina>()), Times.Never);
        unitOfWorkMock?.Verify(u => u.Commit(), Times.Never);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Registro duplicado", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Cadastrar_DeveRetornarFalha_QuandoExcecaoForLancada()
    {
        // Arrange
        var disciplina = new Disciplina("História");

        repositorioDisciplinaMock?
            .Setup(r => r.SelecionarRegistros())
            .Returns(new List<Disciplina>());

        repositorioDisciplinaMock?
            .Setup(r => r.Cadastrar(disciplina))
            .Throws(new Exception("Erro inesperado"));

        // Act
        var resultado = appService?.Cadastrar(disciplina);

        // Assert
        unitOfWorkMock?.Verify(u => u.Rollback(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Ocorreu um erro interno do servidor", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Editar_DeveRetornarOk_QuandoDisciplinaForValida()
    {
        // Arrange
        var id = Guid.NewGuid();
        var disciplinaEditada = new Disciplina("Português") { Id = id };

        repositorioDisciplinaMock?
            .Setup(r => r.SelecionarRegistros())
            .Returns(new List<Disciplina> { new Disciplina("Matemática") });

        // Act
        var resultado = appService?.Editar(id, disciplinaEditada);

        // Assert
        repositorioDisciplinaMock?.Verify(r => r.Editar(id, disciplinaEditada), Times.Once);
        unitOfWorkMock?.Verify(u => u.Commit(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
    }

    [TestMethod]
    public void Editar_DeveRetornarFalha_QuandoNomeDuplicado()
    {
        // Arrange
        var idAtual = Guid.NewGuid();
        var disciplinaEditada = new Disciplina("Matemática") { Id = idAtual };

        var outraDisciplinaComMesmoNome = new Disciplina("Matemática");

        repositorioDisciplinaMock?
            .Setup(r => r.SelecionarRegistros())
            .Returns(new List<Disciplina> { outraDisciplinaComMesmoNome });

        // Act
        var resultado = appService?.Editar(idAtual, disciplinaEditada);

        // Assert
        repositorioDisciplinaMock?.Verify(r => r.Editar(It.IsAny<Guid>(), It.IsAny<Disciplina>()), Times.Never);
        unitOfWorkMock?.Verify(u => u.Commit(), Times.Never);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Registro duplicado", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Editar_DeveRetornarFalha_QuandoExcecaoForLancada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var disciplinaEditada = new Disciplina("História") { Id = id };

        repositorioDisciplinaMock?
            .Setup(r => r.SelecionarRegistros())
            .Returns(new List<Disciplina>());

        repositorioDisciplinaMock?
            .Setup(r => r.Editar(id, disciplinaEditada))
            .Throws(new Exception("Erro interno"));

        // Act
        var resultado = appService?.Editar(id, disciplinaEditada);

        // Assert
        unitOfWorkMock?.Verify(u => u.Rollback(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Ocorreu um erro interno do servidor", resultado.Errors[0].Message);
    }

    [TestMethod]
    public void Excluir_DeveExcluirComSucesso_QuandoNaoHouverDependencias()
    {
        // Arrange
        var disciplinaId = Guid.NewGuid();

        repositorioMateriaMock?.Setup(r => r.SelecionarRegistros())
            .Returns(new List<Materia>());

        repositorioTesteMock?.Setup(r => r.SelecionarRegistros())
            .Returns(new List<Teste>());

        // Act
        var resultado = appService?.Excluir(disciplinaId);

        // Assert
        repositorioDisciplinaMock?.Verify(r => r.Excluir(disciplinaId), Times.Once);
        unitOfWorkMock?.Verify(u => u.Commit(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
    }

    [TestMethod]
    public void Excluir_DeveRetornarFalha_QuandoDisciplinaEstiverEmMaterias()
    {
        // Arrange
        var disciplina = Builder<Disciplina>.CreateNew().Build();
        var materia = Builder<Materia>.CreateNew().With(m => m.Disciplina = disciplina).Build();

        repositorioMateriaMock?.Setup(r => r.SelecionarRegistros())
            .Returns(new List<Materia> { materia });

        // Act
        var resultado = appService?.Excluir(disciplina.Id);

        // Assert
        repositorioDisciplinaMock?.Verify(r => r.Excluir(It.IsAny<Guid>()), Times.Never);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(
            "Exclusão bloqueada",
            resultado.Errors.First().Message
        );
    }
}
