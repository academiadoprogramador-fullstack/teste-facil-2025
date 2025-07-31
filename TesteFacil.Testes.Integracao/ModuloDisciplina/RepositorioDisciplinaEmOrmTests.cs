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
    private static TesteDbContextFactory factory;
    private TesteFacilDbContext dbContext;
    private RepositorioDisciplinaEmOrm repositorioDisciplina;

    [AssemblyInitialize]
    public static async Task Setup(TestContext context)
    {
        factory = new TesteDbContextFactory();

        await factory.InicializarAsync();
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        await factory.EncerrarAsync();
    }

    [TestInitialize]
    public async Task ConfigurarTestes()
    {
        dbContext = await factory.CriarDbContextAsync();

        repositorioDisciplina = new RepositorioDisciplinaEmOrm(dbContext);

        BuilderSetup.SetCreatePersistenceMethod<Disciplina>(repositorioDisciplina.Cadastrar);
        BuilderSetup.SetCreatePersistenceMethod<IList<Disciplina>>(repositorioDisciplina.CadastrarTodos);
    }

    [TestMethod]
    public void Deve_Cadastrar_Disciplina_Corretamente()
    {
        // Arrange
        var disciplina = Builder<Disciplina>.CreateNew().Build();

        // Act
        repositorioDisciplina.Cadastrar(disciplina);
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

        var disciplinaEditada = new Disciplina("Português");

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
