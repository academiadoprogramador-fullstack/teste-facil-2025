using DotNet.Testcontainers.Containers;
using FizzWare.NBuilder;
using Testcontainers.MsSql;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Infraestrutura.Orm.Compartilhado;
using TesteFacil.Infraestrutura.Orm.ModuloDisciplina;
using TesteFacil.Infraestrutura.Orm.ModuloMateria;
using TesteFacil.Infraestrutura.Orm.ModuloQuestao;
using TesteFacil.Infraestrutura.Orm.ModuloTeste;

namespace TesteFacil.Testes.Integracao.Compartilhado;

[TestClass]
public abstract class TestFixture
{
    protected TesteFacilDbContext? dbContext;

    protected RepositorioTesteEmOrm? repositorioTeste;
    protected RepositorioQuestaoEmOrm? repositorioQuestao;
    protected RepositorioMateriaEmOrm? repositorioMateria;
    protected RepositorioDisciplinaEmOrm? repositorioDisciplina;

    private static IDatabaseContainer? dbContainer;

    [AssemblyInitialize]
    public static async Task Setup(TestContext _)
    {
        dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithName("teste-facil-testdb-container")
            .WithCleanUp(true)
            .Build();

        await InicializarContainerBancoDadosAsync(dbContainer);
    }

    [AssemblyCleanup]
    public static async Task Teardown()
    {
        await PararContainerBancoDadosAsync();
    }

    [TestInitialize]
    public void ConfigurarTestes()
    {
        if (dbContainer is null)
            throw new ArgumentNullException("O Banco de dados não foi inicializado.");

        dbContext = TesteFacilDbContextFactory.CriarDbContext(dbContainer.GetConnectionString());

        ConfigurarTabelas(dbContext);

        repositorioTeste = new RepositorioTesteEmOrm(dbContext);
        repositorioQuestao = new RepositorioQuestaoEmOrm(dbContext);
        repositorioDisciplina = new RepositorioDisciplinaEmOrm(dbContext);
        repositorioMateria = new RepositorioMateriaEmOrm(dbContext);

        BuilderSetup.SetCreatePersistenceMethod<Disciplina>(repositorioDisciplina.Cadastrar);
        BuilderSetup.SetCreatePersistenceMethod<IList<Disciplina>>(repositorioDisciplina.CadastrarEntidades);
    }

    private static void ConfigurarTabelas(TesteFacilDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();

        dbContext.Testes.RemoveRange(dbContext.Testes);
        dbContext.Questoes.RemoveRange(dbContext.Questoes);
        dbContext.Materias.RemoveRange(dbContext.Materias);
        dbContext.Disciplinas.RemoveRange(dbContext.Disciplinas);

        dbContext.SaveChanges();
    }

    private static async Task InicializarContainerBancoDadosAsync(IDatabaseContainer dbContainer)
    {
        await dbContainer.StartAsync();
    }

    private static async Task PararContainerBancoDadosAsync()
    {
        if (dbContainer is null)
            throw new ArgumentNullException("O Banco de dados não foi inicializado.");

        await dbContainer.StopAsync();
        await dbContainer.DisposeAsync();
    }
}
