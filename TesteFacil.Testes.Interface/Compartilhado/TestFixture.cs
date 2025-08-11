using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TesteFacil.Infraestrutura.Orm.Compartilhado;
using TesteFacil.Testes.Integracao.Compartilhado;

namespace TesteFacil.Testes.Interface.Compartilhado;

[TestClass]
public abstract class TestFixture
{
    protected static IWebDriver? driver;
    protected TesteFacilDbContext? dbContext;

    protected static string enderecoBase = "https://localhost:7056";
    private static string connectionString = "Host=localhost;Port=5432;Database=TesteFacilDb;Username=postgres;Password=YourStrongPassword";

    [TestInitialize]
    public void ConfigurarTestes()
    {
        dbContext = TesteFacilDbContextFactory.CriarDbContext(connectionString);

        ConfigurarTabelas(dbContext);

        InicializarWebDriver();
    }

    [TestCleanup]
    public void Cleanup()
    {
        EncerrarWebDriver();
    }

    private static void InicializarWebDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");

        driver = new ChromeDriver(options);
    }

    private static void EncerrarWebDriver()
    {
        driver?.Quit();
        driver?.Dispose();
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
}
