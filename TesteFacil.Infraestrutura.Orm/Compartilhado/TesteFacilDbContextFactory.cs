using Microsoft.EntityFrameworkCore;

namespace TesteFacil.Infraestrutura.Orm.Compartilhado;

public static class TesteFacilDbContextFactory
{
    public static TesteFacilDbContext CriarDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TesteFacilDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var dbContext = new TesteFacilDbContext(options);

        return dbContext;
    }
}
