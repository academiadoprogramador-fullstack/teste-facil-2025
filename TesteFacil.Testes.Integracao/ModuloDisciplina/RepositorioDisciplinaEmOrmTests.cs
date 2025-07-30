using FizzWare.NBuilder;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Infraestrutura.Orm.Compartilhado;
using TesteFacil.Infraestrutura.Orm.ModuloDisciplina;
using TesteFacil.Testes.Integracao.Compartilhado;

namespace TesteFacil.Testes.Integracao.ModuloDisciplina;

[TestClass]
[TestCategory("Testes de Integração de Disciplina")]
public sealed class RepositorioDisciplinaEmOrmTests
{
    private TesteFacilDbContext dbContext;
    private RepositorioDisciplinaEmOrm repositorioDisciplina;

    [TestInitialize]
    public void ConfigurarTestes()
    {
        dbContext = TesteDbContextFactory.CriarDbContext();
       
        repositorioDisciplina = new RepositorioDisciplinaEmOrm(dbContext);

        BuilderSetup.SetCreatePersistenceMethod<Disciplina>(repositorioDisciplina.Cadastrar);
        BuilderSetup.SetCreatePersistenceMethod<IList<Disciplina>>(repositorioDisciplina.CadastrarTodos);
    }

    [TestMethod]
    public void Deve_Cadastrar_Disciplina_Corretamente()
    {
        // Arrange
        var disciplina = Builder<Disciplina>.CreateNew().Persist();

        // Act
        dbContext.SaveChanges();

        // Assert
        var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(disciplina.Id);
        
        Assert.AreEqual(disciplina, registroSelecionado);
    }

    [TestMethod]
    public void Deve_Editar_Disciplina_Corretamente()
    {
        // Arrange
        var disciplina = Builder<Disciplina>.CreateNew().Persist();

        dbContext.SaveChanges();

        var disciplinaEditada = Builder<Disciplina>.CreateNew().Build();

        // Act
        var conseguiuEditar = repositorioDisciplina.Editar(disciplina.Id, disciplinaEditada);
        dbContext.SaveChanges();

        // Assert
        var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(disciplina.Id);

        Assert.IsTrue(conseguiuEditar);
        Assert.AreEqual(disciplina, registroSelecionado);
    }

    [TestMethod]
    public void Deve_Excluir_Disciplina_Corretamente()
    {
        // Arrange
        var disciplina = Builder<Disciplina>.CreateNew().Persist();
        dbContext.SaveChanges();

        // Act
        var conseguiuExcluir = repositorioDisciplina.Excluir(disciplina.Id);
        dbContext.SaveChanges();

        // Assert
        var registroSelecionado = repositorioDisciplina.SelecionarRegistroPorId(disciplina.Id);

        Assert.IsTrue(conseguiuExcluir);
        Assert.IsNull(registroSelecionado);
    }

    [TestMethod]
    public void Deve_Selecionar_Disciplinas_Corretamente()
    {
        // Arrange - Arranjo
        var disciplinasEsperadas = Builder<Disciplina>.CreateListOfSize(3).Persist().ToList();

        dbContext.SaveChanges();

        var disciplinasEsperadasOrdenadas = disciplinasEsperadas
            .OrderBy(d => d.Nome)
            .ToList();

        // Act - Ação
        var disciplinasRecebidas = repositorioDisciplina
            .SelecionarRegistros();

        // Assert - Asseção
        CollectionAssert.AreEqual(disciplinasEsperadasOrdenadas, disciplinasRecebidas);
    }
}
