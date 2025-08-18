using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Testcontainers.PostgreSql;
using TesteFacil.Infraestrutura.Orm.Compartilhado;

namespace TesteFacil.Testes.Interface.Compartilhado;

[TestClass]
public abstract class TestFixture
{
    protected static IWebDriver? driver;
    protected static string? enderecoBase;

    private static IConfiguration? configuracao;
    private static DotNet.Testcontainers.Networks.INetwork? rede;

    private static IDatabaseContainer? dbContainer;
    private readonly static int dbPort = 5432;

    private static IContainer? appContainer;
    private readonly static int appPort = 8080;

    private static IContainer? seleniumContainer;
    private readonly static int seleniumPort = 4444;

    private static TesteFacilDbContext? dbContext;

    [AssemblyInitialize]
    public static async Task ConfigurarTestes(TestContext _)
    {
        configuracao = new ConfigurationBuilder()
            .AddUserSecrets<TestFixture>()
            .AddEnvironmentVariables()
            .Build();

        rede = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .WithCleanUp(true)
            .Build();

        await InicializarBancoDadosAsync();

        await InicializarAplicacaoAsync();

        await InicializarWebDriverAsync();
    }

    [AssemblyCleanup]
    public static async Task EncerrarTestes()
    {
        EncerrarWebDriverAsync();

        await EncerrarAplicacaoAsync();

        await EncerrarBancoDadosAsync();
    }

    [TestInitialize]
    public void InicializarTeste()
    {
        if (dbContainer is null)
            throw new ArgumentNullException("O banco de dados não foi inicializado.");

        dbContext = TesteFacilDbContextFactory.CriarDbContext(dbContainer.GetConnectionString());

        ConfigurarTabelas(dbContext);
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

    private static async Task InicializarBancoDadosAsync()
    {
        dbContainer = new PostgreSqlBuilder()
          .WithImage("postgres:16")
          .WithPortBinding(dbPort, true)
          .WithNetwork(rede)
          .WithNetworkAliases("teste-facil-e2e-testdb")
          .WithName("teste-facil-e2e-testdb")
          .WithDatabase("TesteFacilTestDb")
          .WithUsername("postgres")
          .WithPassword("YourStrongPassword")
          .WithCleanUp(true)
          .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilPortIsAvailable(dbPort)
          )
          .Build();

        await dbContainer.StartAsync();

    }

    private static async Task InicializarAplicacaoAsync()
    {
        // Configura a imagem à partir do Dockerfile
        var imagem = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("Dockerfile")
            .WithBuildArgument("RESOURCE_REAPER_SESSION_ID", ResourceReaper.DefaultSessionId.ToString("D"))
            .WithName("teste-facil-app-e2e:latest")
            .Build();

        await imagem.CreateAsync().ConfigureAwait(false);

        // Configura a connection string para a rede:
        // "Host=teste-facil-e2e-testdb;Port=5432;Database=TesteFacilDb;Username=postgres;Password=YourStrongPassword"
        var connectionStringRede = dbContainer?.GetConnectionString()
            .Replace(dbContainer.Hostname, "teste-facil-e2e-testdb")
            .Replace(dbContainer.GetMappedPublicPort(dbPort).ToString(), dbPort.ToString());

        // Configura o container da aplicação e inicializa o enderecoBase
        appContainer = new ContainerBuilder()
            .WithImage(imagem)
            .WithPortBinding(appPort, true)
            .WithNetwork(rede)
            .WithNetworkAliases("teste-facil-webapp")
            .WithName("teste-facil-webapp")
            .WithEnvironment("SQL_CONNECTION_STRING", connectionStringRede)
            .WithEnvironment("GEMINI_API_KEY", configuracao?["GEMINI_API_KEY"])
            .WithEnvironment("NEWRELIC_LICENSE_KEY", configuracao?["NEWRELIC_LICENSE_KEY"])
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(appPort)
                .UntilHttpRequestIsSucceeded(r => r.ForPort((ushort)appPort).ForPath("/health"))
            )
            .WithCleanUp(true)
            .Build();

        await appContainer.StartAsync();

        // URL interno: http://teste-facil-webapp:8080
        enderecoBase = $"http://{appContainer.Name}:{appPort}";
    }

    private static async Task InicializarWebDriverAsync()
    {
        seleniumContainer = new ContainerBuilder()
            .WithImage("selenium/standalone-chrome:nightly")
            .WithPortBinding(seleniumPort, true)
            .WithNetwork(rede)
            .WithNetworkAliases("teste-facil-selenium-e2e")
            .WithExtraHost("host.docker.internal", "host-gateway")
            .WithName("teste-facil-selenium-e2e")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(seleniumPort)
            )
            .Build();

        await seleniumContainer.StartAsync();

        var enderecoSelenium = new Uri($"http://{seleniumContainer.Hostname}:{seleniumContainer.GetMappedPublicPort(seleniumPort)}/wd/hub");

        var options = new ChromeOptions();
        options.AddArgument("--headless=new");

        driver = new RemoteWebDriver(enderecoSelenium, options);
    }

    private static async Task EncerrarBancoDadosAsync()
    {
        if (dbContainer is not null)
            await dbContainer.DisposeAsync();
    }

    private static async Task EncerrarAplicacaoAsync()
    {
        if (appContainer is not null)
            await appContainer.DisposeAsync();
    }

    private static async Task EncerrarWebDriverAsync()
    {
        driver?.Quit();
        driver?.Dispose();

        if (seleniumContainer is not null)
            await seleniumContainer.DisposeAsync();
    }
}
