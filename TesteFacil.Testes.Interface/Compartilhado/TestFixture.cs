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

    private static IContainer? appContainer;
    private static IDatabaseContainer? dbContainer;
    private static IContainer? seleniumContainer;

    private static IConfiguration? configuracao;
    private TesteFacilDbContext? dbContext;

    [AssemblyInitialize]
    public static async Task ConfigurarTestes(TestContext _)
    {
        configuracao = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<TestFixture>()
            .Build();

        var rede = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .WithCleanUp(true)
            .Build();

        await rede.CreateAsync().ConfigureAwait(false);

        await InicializarPostgreSqlAsync(rede);

        await InicializarAplicacaoAsync(rede);

        await InicializarSeleniumAsync(rede);
    }

    [AssemblyCleanup]
    public static async Task EncerrarTestes()
    {
        await EncerrarSeleniumAsync();

        await EncerrarAplicacaoAsync();

        await EncerrarPostgreSqlAsync();
    }

    [TestInitialize]
    public async Task ConfigurarTeste()
    {
        if (dbContainer is null)
            throw new ArgumentNullException("O banco de dados não foi inicializado.");

        dbContext = TesteFacilDbContextFactory.CriarDbContext(dbContainer.GetConnectionString());

        await ConfigurarTabelasAsync(dbContext);
    }

    private static async Task ConfigurarTabelasAsync(TesteFacilDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Testes.RemoveRange(dbContext.Testes);
        dbContext.Questoes.RemoveRange(dbContext.Questoes);
        dbContext.Materias.RemoveRange(dbContext.Materias);
        dbContext.Disciplinas.RemoveRange(dbContext.Disciplinas);

        await dbContext.SaveChangesAsync();
    }

    private static async Task InicializarPostgreSqlAsync(DotNet.Testcontainers.Networks.INetwork network)
    {
        // Configura e inicializa o container do banco de dados
        dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithPortBinding(5432, true)
            .WithName("teste-facil-e2e-testdb")
            .WithDatabase("TesteFacilTestDb")
            .WithUsername("postgres")
            .WithPassword("YourStrongPassword")
            .WithNetwork(network)
            .WithNetworkAliases("teste-facil-e2e-testdb")
            .WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5432)
            )
            .Build();

        await dbContainer.StartAsync();
    }

    private static async Task InicializarAplicacaoAsync(DotNet.Testcontainers.Networks.INetwork network)
    {
        // Configura a imagem à partir do Dockerfile
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("Dockerfile")
            .WithBuildArgument("RESOURCE_REAPER_SESSION_ID", ResourceReaper.DefaultSessionId.ToString("D"))
            .WithName("teste-facil-app-e2e:latest")
            .Build();

        await image.CreateAsync().ConfigureAwait(false);

        // Configura a connection string para o network
        var networkConnectionString = dbContainer?.GetConnectionString()
            .Replace(dbContainer.Hostname, "teste-facil-e2e-testdb")
            .Replace(dbContainer.GetMappedPublicPort(5432).ToString(), "5432");

        // Configura o container da aplicação e inicializa o enderecoBase
        appContainer = new ContainerBuilder()
            .WithImage(image)
            .WithPortBinding(8080, true)
            .WithName("teste-facil-webapp")
            .WithNetwork(network)
            .WithNetworkAliases("teste-facil-webapp")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Testing")
            .WithEnvironment("SQL_CONNECTION_STRING", networkConnectionString)
            .WithEnvironment("GEMINI_API_KEY", configuracao?["GEMINI_API_KEY"])
            .WithEnvironment("NEWRELIC_LICENSE_KEY", configuracao?["NEWRELIC_LICENSE_KEY"])
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(8080)
                .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health"))
            )
            .WithCleanUp(true)
            .Build();

        await appContainer.StartAsync();

        enderecoBase = $"http://{appContainer.Name}:8080";
    }

    private static async Task InicializarSeleniumAsync(DotNet.Testcontainers.Networks.INetwork network)
    {
        // Configura o container do selenium e o navegador chrome remoto
        seleniumContainer = new ContainerBuilder()
            .WithImage("selenium/standalone-chrome:nightly")
            .WithPortBinding(4444, true)
            .WithName("teste-facil-selenium-e2e")
            .WithNetwork(network)
            .WithNetworkAliases("teste-facil-selenium-e2e")
            .WithExtraHost("host.docker.internal", "host-gateway")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4444))
            .Build();

        await seleniumContainer.StartAsync();

        var enderecoSelenium = $"http://{seleniumContainer.Hostname}:{seleniumContainer.GetMappedPublicPort(4444)}/wd/hub";

        var options = new ChromeOptions();

        options.AddArguments(
            "--headless=new",
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--window-size=1920,1080"
        );

        driver = new RemoteWebDriver(new Uri(enderecoSelenium), options);
    }

    private static async Task EncerrarPostgreSqlAsync()
    {
        if (dbContainer is not null)
            await dbContainer.DisposeAsync();
    }

    private static async Task EncerrarAplicacaoAsync()
    {
        if (appContainer is not null)
            await appContainer.DisposeAsync();
    }

    private static async Task EncerrarSeleniumAsync()
    {
        driver?.Quit();
        driver?.Dispose();

        if (seleniumContainer is not null)
            await seleniumContainer.DisposeAsync();
    }
}