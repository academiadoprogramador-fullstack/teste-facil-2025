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
    protected readonly static string enderecoDriver = "http://host.docker.internal:5239";

    private static IConfiguration? configuracao;

    private static IDatabaseContainer? dbContainer;
    private static IContainer? seleniumContainer;

    private TesteFacilDbContext? dbContext;
    private static Process? processoAplicacao;
    protected readonly static string enderecoBase = "http://localhost:5239";

    [AssemblyInitialize]
    public static async Task ConfigurarTestes(TestContext _)
    {
        configuracao = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<TestFixture>()
            .Build();

        var network = new NetworkBuilder()
            .WithName("teste-facil-network")
            .Build();

        await network.CreateAsync();

        await InicializarPostgreSqlAsync(network);

        await InicializarAplicacaoAsync();

        await InicializarSeleniumAsync(network);
    }

    [AssemblyCleanup]
    public static async Task EncerrarTestes()
    {
        await EncerrarSeleniumAsync();

        EncerrarAplicacao();

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

    private static async Task InicializarAplicacaoAsync()
    {
        try
        {
            // Configura o processo que irá executar o projeto Web
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project ../../../../TesteFacil.WebApp",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            // Adiciona variáveis de ambiente específicas para o processo
            processStartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
            processStartInfo.Environment["ASPNETCORE_URLS"] = "http://0.0.0.0:5239";
            processStartInfo.Environment["SQL_CONNECTION_STRING"] = dbContainer?.GetConnectionString();
            processStartInfo.Environment["GEMINI_API_KEY"] = configuracao?["GEMINI_API_KEY"];
            processStartInfo.Environment["NEWRELIC_LICENSE_KEY"] = configuracao?["NEWRELIC_LICENSE_KEY"];
            
            // Inicializa o processo da aplicação
            processoAplicacao = Process.Start(processStartInfo);

            if (processoAplicacao is null)
                throw new InvalidOperationException("Não foi possível iniciar o processo da aplicação");

            Debug.WriteLine($"Aplicação iniciada com PID: {processoAplicacao.Id}");

            // Conecta eventos de log do processo ao debug do teste
            processoAplicacao.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.WriteLine(e.Data);
            };

            processoAplicacao.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.WriteLine("ERR: " + e.Data);
            };

            processoAplicacao.BeginOutputReadLine();
            processoAplicacao.BeginErrorReadLine();

            await EsperarAplicacao();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao iniciar a aplicação: {ex.Message}", ex);
        }
    }

    private static async Task EsperarAplicacao()
    {
        using var httpClient = new HttpClient();

        var inicio = DateTime.UtcNow;
        var limite = TimeSpan.FromSeconds(30);

        while (DateTime.UtcNow - inicio < limite)
        {
            try
            {
                var res = await httpClient.GetAsync($"{enderecoBase}/health");

                if (res.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // Ignora erros de conexão enquanto a app sobe
            }

            await Task.Delay(500);
        }

        throw new Exception("Aplicação não ficou pronta no tempo limite.");
    }

    private static void EncerrarAplicacao()
    {
        try
        {
            if (processoAplicacao is not null && !processoAplicacao.HasExited)
            {
                processoAplicacao.Kill();
                processoAplicacao.WaitForExit(5000);
                processoAplicacao.Dispose();

                Debug.WriteLine("Processo da aplicação encerrado.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao encerrar aplicação: {ex.Message}.");
        }
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