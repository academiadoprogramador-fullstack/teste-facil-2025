using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Infraestrutura.Orm.Compartilhado;
using TesteFacil.Infraestrutura.Orm.ModuloDisciplina;
using TesteFacil.Testes.Integracao.Compartilhado;

namespace TesteFacil.Testes.Integracao.ModuloDisciplina;

[TestClass]
[TestCategory("Testes de Integração de Disciplina")]
public sealed class RepositorioDisciplinaEmOrmTests
{
    private TesteFacilDbContext? dbContext;
    private RepositorioDisciplinaEmOrm? repositorioDisciplina;

    [TestInitialize]
    public void ConfigurarTestes()
    {
        dbContext = DbContextFactory.CriarDbContext();

        repositorioDisciplina = new RepositorioDisciplinaEmOrm(dbContext);
    }

    [TestMethod]
    public void Deve_Cadastrar_Registro_Corretamente()
    {
        // Arrange (Configuração)
        var entidade = new Disciplina("Ciências");

        // Act (Ação)
        repositorioDisciplina?.Cadastrar(entidade);

        dbContext?.SaveChanges();

        // Assert (Asserção)
        var registroSelecionado = repositorioDisciplina?.SelecionarRegistroPorId(entidade.Id);

        Assert.AreEqual(entidade, registroSelecionado);
    }

    [TestMethod]
    public void Deve_Editar_Registro_Corretamente()
    {
        // Arrange (Configuração)
        var entidade = new Disciplina("Ciências");
        
        repositorioDisciplina?.Cadastrar(entidade);
        dbContext?.SaveChanges();

        // Act (Ação)
        var entidadeEditada = new Disciplina("Física");

        var conseguiuEditar = repositorioDisciplina?.Editar(entidade.Id, entidadeEditada);
        dbContext?.SaveChanges();

        // Assert (Asserção)
        var registroSelecionado = repositorioDisciplina?.SelecionarRegistroPorId(entidade.Id);

        Assert.IsTrue(conseguiuEditar);
        Assert.AreEqual(entidade, registroSelecionado);
    }

    [TestMethod]
    public void Deve_Excluir_Registro_Corretamente()
    {
        // Arrange (Configuração)
        var entidade = new Disciplina("Ciências");

        repositorioDisciplina?.Cadastrar(entidade);
        dbContext?.SaveChanges();

        // Act (Ação)
        var conseguiuExcluir = repositorioDisciplina?.Excluir(entidade.Id);
        dbContext?.SaveChanges();

        // Assert (Asserção)
        var registroSelecionado = repositorioDisciplina?.SelecionarRegistroPorId(entidade.Id);

        Assert.IsTrue(conseguiuExcluir);
        Assert.IsNull(registroSelecionado);
    }

    [TestMethod]
    public void Deve_Selecionar_Registros_Corretamente()
    {
        // Arrange (Configuração)
        var entidade = new Disciplina("Ciências");
        var entidade2 = new Disciplina("Artes");
        var entidade3 = new Disciplina("Português");

        List<Disciplina> entidades = [entidade, entidade2, entidade3];

        repositorioDisciplina?.Cadastrar(entidade);
        repositorioDisciplina?.Cadastrar(entidade2);
        repositorioDisciplina?.Cadastrar(entidade3);
        dbContext?.SaveChanges();

        // Act (Ação)
        var registros = repositorioDisciplina?.SelecionarRegistros();

        // Assert (Asserção)
        CollectionAssert.AreEqual(entidades, registros);
    }
}
