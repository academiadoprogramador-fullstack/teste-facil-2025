using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;
using TesteFacil.Infraestrutura.Orm.Compartilhado;

namespace TesteFacil.Testes.Integracao.Compartilhado;

public class TesteDbContextFactory
{
    private readonly MsSqlContainer _container;

    public TesteDbContextFactory()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithCleanUp(true)
            .Build();
    }

    public async Task<TesteFacilDbContext> CriarDbContextAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<TesteFacilDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        var dbContext = new TesteFacilDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
