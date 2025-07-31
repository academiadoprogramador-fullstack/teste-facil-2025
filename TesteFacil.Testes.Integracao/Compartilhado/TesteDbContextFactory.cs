using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using TesteFacil.Infraestrutura.Orm.Compartilhado;

namespace TesteFacil.Testes.Integracao.Compartilhado;

public class TesteDbContextFactory
{
    private readonly MsSqlContainer container;
    private string connectionString;

    public TesteDbContextFactory()
    {
        container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithCleanUp(true)
            .WithName("teste-facil-testdb-container")
            .Build();
    }

    public async Task<TesteFacilDbContext> CriarDbContextAsync()
    {
        connectionString = string.Concat(container.GetConnectionString(), $";Initial Catalog=TesteFacilTestDb");

        var options = new DbContextOptionsBuilder<TesteFacilDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var dbContext = new TesteFacilDbContext(options);

        await ConfigurarDbContext(dbContext);

        return dbContext;
    }

    public async Task InicializarAsync()
    {
        await container.StartAsync();
    }

    public async Task EncerrarAsync()
    {
        await container.StopAsync();
        await container.DisposeAsync();
    }

    private static async Task ConfigurarDbContext(TesteFacilDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Testes.RemoveRange(dbContext.Testes);
        dbContext.Questoes.RemoveRange(dbContext.Questoes);
        dbContext.Materias.RemoveRange(dbContext.Materias);
        dbContext.Disciplinas.RemoveRange(dbContext.Disciplinas);

        await dbContext.SaveChangesAsync();
    }
}
