using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TesteFacil.Infraestrutura.Orm.Compartilhado;

namespace TesteFacil.Testes.Integracao.Compartilhado;

public static class DbContextFactory
{
    private static readonly IConfiguration configuracao = CriarConfiguracao();

    public static TesteFacilDbContext CriarDbContext()
    {
        var connectionString = configuracao["SQL_CONNECTION_STRING"];

        var options = new DbContextOptionsBuilder<TesteFacilDbContext>()
            .UseNpgsql(connectionString) 
            .Options;

        var dbContext = new TesteFacilDbContext(options);

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        return dbContext;
    }

    private static IConfiguration CriarConfiguracao()
    {
        return new ConfigurationBuilder()
            .AddUserSecrets(typeof(DbContextFactory).Assembly)
            .AddEnvironmentVariables()
            .Build();
    }
}
