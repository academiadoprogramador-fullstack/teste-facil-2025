using TesteFacil.Dominio.ModuloTeste;
using TesteFacil.Infraestrutura.Orm.Compartilhado;

namespace TesteFacil.Infraestrutura.Orm.ModuloTeste;

public class RepositorioTesteEmOrm : RepositorioBaseEmOrm<Teste>, IRepositorioTeste
{
    public RepositorioTesteEmOrm(TesteFacilDbContext contexto) : base(contexto)
    {
    }
}