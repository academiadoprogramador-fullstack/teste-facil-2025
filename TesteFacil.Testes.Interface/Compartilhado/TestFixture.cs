using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Diagnostics;
using Testcontainers.PostgreSql;
using TesteFacil.Infraestrutura.Orm.Compartilhado;

namespace TesteFacil.Testes.Interface.Compartilhado;

[TestClass]
public abstract class TestFixture
{
    protected static IWebDriver? driver;

    private static IConfiguration? configuracao;

    private static IContainer? appContainer;
    private static IDatabaseContainer? dbContainer;
    private static IContainer? seleniumContainer;

    private TesteFacilDbContext? dbContext;
    protected static string? enderecoBase;

    [AssemblyInitialize]
    public static async Task ConfigurarTestes(TestContext _)
    {
        configuracao = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<TestFixture>()
            .Build();

        var network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .WithCleanUp(true)
            .Build();

        await network.CreateAsync().ConfigureAwait(false);

        await InicializarPostgreSqlAsync(network);

        await InicializarContainerAplicacaoAsync(network);

        await InicializarSeleniumAsync(network);
    }

    [AssemblyCleanup]
    public static async Task EncerrarTestes()
    {
        await EncerrarSeleniumAsync();

        await EncerrarContainerAplicacaoAsync();

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
        dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithName("teste-facil-e2e-testdb")
            .WithDatabase("TesteFacilTestDb")
            .WithUsername("postgres")
            .WithPassword("YourStrongPassword")
            .WithNetwork(network)
            .WithNetworkAliases("db")
            .WithCleanUp(true)
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        await dbContainer.StartAsync();

        Debug.WriteLine($"PostgreSQL Container iniciado na porta: {dbContainer.GetMappedPublicPort(5432)}");
        Debug.WriteLine($"Connection String: {dbContainer.GetConnectionString()}");
    }

    private static async Task EncerrarPostgreSqlAsync()
    {
        if (dbContainer != null)
            await dbContainer.DisposeAsync();
    }

    private static async Task InicializarContainerAplicacaoAsync(DotNet.Testcontainers.Networks.INetwork network)
    {
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("Dockerfile")
            .WithBuildArgument("RESOURCE_REAPER_SESSION_ID", ResourceReaper.DefaultSessionId.ToString("D"))
            .WithName("teste-facil-app-e2e:latest")
            .Build();

        await image.CreateAsync().ConfigureAwait(false);

        var networkConnectionString = dbContainer?.GetConnectionString()
            .Replace(dbContainer.Hostname, "db")
            .Replace(dbContainer.GetMappedPublicPort(5432).ToString(), "5432");

        appContainer = new ContainerBuilder()
            .WithImage(image)
            .WithName("teste-facil-webapp")
            .WithPortBinding(8080, true)
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

        Debug.WriteLine($"http://{appContainer.Hostname}:{appContainer.GetMappedPublicPort(8080)}");

        enderecoBase = "http://teste-facil-webapp:8080";
    }

    private static async Task EncerrarContainerAplicacaoAsync()
    {
        if (appContainer is not null)
            await appContainer.DisposeAsync();
    }

    private static async Task InicializarSeleniumAsync(DotNet.Testcontainers.Networks.INetwork network)
    {
        seleniumContainer = new ContainerBuilder()
            .WithImage("selenium/standalone-chrome:nightly")
            .WithName("teste-facil-selenium-e2e")
            .WithNetwork(network)
            .WithNetworkAliases("selenium")
            .WithExtraHost("host.docker.internal", "host-gateway")
            .WithPortBinding(4444, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4444))
            .Build();

        await seleniumContainer.StartAsync();

        var seleniumUrl = $"http://{seleniumContainer.Hostname}:{seleniumContainer.GetMappedPublicPort(4444)}/wd/hub";

        var options = new ChromeOptions();

        options.AddArguments(
            "--headless",
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--window-size=1920,1080"
        );

        driver = new RemoteWebDriver(new Uri(seleniumUrl), options);
    }

    private static async Task EncerrarSeleniumAsync()
    {
        try
        {
            driver?.Quit();
            driver?.Dispose();

            if (seleniumContainer is not null)
                await seleniumContainer.DisposeAsync();

            Debug.WriteLine("Selenium encerrado.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao encerrar Selenium: {ex.Message}.");
        }
    }
}