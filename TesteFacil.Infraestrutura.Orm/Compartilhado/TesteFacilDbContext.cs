using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TesteFacil.Dominio.Compartilhado;
using TesteFacil.Dominio.ModuloAutenticacao;
using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Dominio.ModuloQuestao;
using TesteFacil.Dominio.ModuloTeste;


namespace TesteFacil.Infraestrutura.Orm.Compartilhado;

public class TesteFacilDbContext : IdentityDbContext<Usuario, Cargo, Guid>, IUnitOfWork
{
    public DbSet<Disciplina> Disciplinas { get; set; }
    public DbSet<Materia> Materias { get; set; }
    public DbSet<Questao> Questoes { get; set; }
    public DbSet<Alternativa> Alternativas { get; set; }
    public DbSet<Teste> Testes { get; set; }

    private readonly ITenantProvider? tenantProvider;

    public TesteFacilDbContext(DbContextOptions options, ITenantProvider? tenantProvider = null) : base(options)
    {
        this.tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (tenantProvider is not null)
        {
            modelBuilder.Entity<Disciplina>()
                .HasQueryFilter(x => x.UsuarioId == tenantProvider.UsuarioId);

            modelBuilder.Entity<Materia>()
                .HasQueryFilter(x => x.UsuarioId == tenantProvider.UsuarioId);

            modelBuilder.Entity<Questao>()
                .HasQueryFilter(x => x.UsuarioId == tenantProvider.UsuarioId);

            modelBuilder.Entity<Alternativa>()
                .HasQueryFilter(x => x.UsuarioId == tenantProvider.UsuarioId);

            modelBuilder.Entity<Teste>()
                .HasQueryFilter(x => x.UsuarioId == tenantProvider.UsuarioId);
        }

        var assembly = typeof(TesteFacilDbContext).Assembly;

        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        base.OnModelCreating(modelBuilder);
    }

    public void Commit()
    {
        SaveChanges();
    }

    public void Rollback()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Unchanged;
                    break;

                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }
    }
}
