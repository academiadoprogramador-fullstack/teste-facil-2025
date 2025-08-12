using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using Testcontainers.PostgreSql;
using TesteFacil.Infraestrutura.Orm.Compartilhado;

namespace TesteFacil.Testes.Interface.Compartilhado;

[TestClass]
public abstract class TestFixture
{
    protected static IWebDriver? driver;
    protected static TesteFacilDbContext? dbContext;
    protected readonly static string enderecoBase = "https://localhost:7056";

    private static IDatabaseContainer? dbContainer;
    private static IConfiguration? configuracao;
    private static Process? processoAplicacao;

    [AssemblyInitialize]
    public static async Task ConfigurarTestes(TestContext _)
    {
        configuracao = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<TestFixture>()
            .Build();

        await InicializarPostgreSqlAsync();

        await InicializarAplicacaoAsync();

        InicializarWebDriver();
    }

    [AssemblyCleanup]
    public static async Task EncerrarTestes()
    {
        EncerrarWebDriver();

        EncerrarAplicacao();

        await EncerrarPostgreSqlAsync();
    }

    [TestInitialize]
    public void ConfigurarTeste()
    {
        if (dbContainer is null)
            throw new ArgumentNullException("O banco de dados não foi inicializado.");

        dbContext = TesteFacilDbContextFactory.CriarDbContext(dbContainer.GetConnectionString());

        ConfigurarTabelas(dbContext);
    }

    private static void ConfigurarTabelas(TesteFacilDbContext dbContext)
    {
        dbContext.Testes.RemoveRange(dbContext.Testes);
        dbContext.Questoes.RemoveRange(dbContext.Questoes);
        dbContext.Materias.RemoveRange(dbContext.Materias);
        dbContext.Disciplinas.RemoveRange(dbContext.Disciplinas);

        dbContext.SaveChanges();
    }

    private static async Task InicializarPostgreSqlAsync()
    {
        dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithName("teste-facil-e2e-testdb")
            .WithDatabase("TesteFacilTestDb")
            .WithUsername("postgres")
            .WithPassword("YourStrongPassword")
            .WithCleanUp(true)
            .WithPortBinding(5432, true) // porta aleatória no host
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
                Arguments = "run --project ../../../../TesteFacil.WebApp --launch-profile \"https [Dev]\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            // Adiciona variáveis de ambiente específicas para o processo
            processStartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
            processStartInfo.Environment["ASPNETCORE_URLS"] = enderecoBase;
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

            Thread.Sleep(500);
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

    private static void InicializarWebDriver()
    {
        var options = new ChromeOptions();

        options.AddArguments(
            "--headless",
            "--ignore-certificate-errors",
            "--disable-dev-shm-usage",                 // Superar limitações de recursos
            "--disable-gpu",                           // Desabilitar GPU em headless,
            "--disable-features=VizDisplayCompositor"  // Melhor compatibilidade
        );

        driver = new ChromeDriver(options);

        Debug.WriteLine("Selenium WebDriver iniciado.");
    }

    private static void EncerrarWebDriver()
    {
        try
        {
            driver?.Quit();
            driver?.Dispose();

            Debug.WriteLine("Selenium WebDriver encerrado.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao encerrar Selenium WebDriver: {ex.Message}.");
        }
    }

    #region Selenium Testcontainer
    //private static async Task InicializarSeleniumAsync()
    //{
    //    seleniumContainer = new ContainerBuilder()
    //        .WithImage("selenium/standalone-chrome:nightly")
    //        .WithName("teste-facil-selenium-e2e")
    //        .WithPortBinding(4444, true) // porta aleatória no host
    //        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4444))
    //        .Build();

    //    await seleniumContainer.StartAsync();

    //    var seleniumHost = seleniumContainer.Hostname;
    //    var mappedPort = seleniumContainer.GetMappedPublicPort(4444);
    //    var seleniumUrl = $"http://{seleniumHost}:{mappedPort}/wd/hub";

    //    var options = new ChromeOptions();
    //    options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");

    //    driver = new RemoteWebDriver(new Uri(seleniumUrl), options);
    //}

    //private static async Task EncerrarSeleniumAsync()
    //{
    //    try
    //    {
    //        driver?.Quit();
    //        driver?.Dispose();

    //        if (seleniumContainer is not null)
    //            await seleniumContainer.DisposeAsync();

    //        Debug.WriteLine("Selenium encerrado.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Erro ao encerrar Selenium: {ex.Message}.");
    //    }
    //}
    #endregion
}