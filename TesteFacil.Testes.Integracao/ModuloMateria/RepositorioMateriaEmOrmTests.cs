using TesteFacil.Dominio.ModuloDisciplina;
using TesteFacil.Dominio.ModuloMateria;
using TesteFacil.Infraestrutura.Orm.Compartilhado;
using TesteFacil.Infraestrutura.Orm.ModuloDisciplina;
using TesteFacil.Infraestrutura.Orm.ModuloMateria;
using TesteFacil.Testes.Integracao.Compartilhado;

namespace TesteFacil.Testes.Integracao.ModuloMateria;

[TestClass]
[TestCategory("Testes de Integração de Matéria")]
public class RepositorioMateriaEmOrmTests
{
    private TesteFacilDbContext? dbContext;
    private RepositorioMateriaEmOrm? repositorioMateria;
    private RepositorioDisciplinaEmOrm? repositorioDisciplina;

    [TestInitialize]
    public void ConfigurarTestes()
    {
        dbContext = DbContextFactory.CriarDbContext();

        repositorioMateria = new RepositorioMateriaEmOrm(dbContext);
        repositorioDisciplina = new RepositorioDisciplinaEmOrm(dbContext);
    }

    [TestMethod]
    public void Deve_Cadastrar_Registro_Corretamente()
    {
        // Arrange (Arranjo)
        var disciplina = new Disciplina("Ciências");
        repositorioDisciplina?.Cadastrar(disciplina);

        dbContext?.SaveChanges();

        var materia = new Materia("Geologia", SerieMateria.SegundaSerie, disciplina);

        // Act (Ação)
        repositorioMateria?.Cadastrar(materia);
        dbContext?.SaveChanges();

        // Assert (Asserção)
        var materiaSelecionada = repositorioMateria?.SelecionarRegistroPorId(materia.Id);

        Assert.AreEqual(materia, materiaSelecionada);
    }

    [TestMethod]
    public void Deve_Editar_Registro_Corretamente()
    {
        // Arrange (Arranjo)
        var disciplina = new Disciplina("Ciências");
        repositorioDisciplina?.Cadastrar(disciplina);

        var materia = new Materia("Geologia", SerieMateria.SegundaSerie, disciplina);
        repositorioMateria?.Cadastrar(materia);

        dbContext?.SaveChanges();

        // Act (Ação)
        var materiaEditada = new Materia("Reinos Animais", SerieMateria.PrimeiraSerie, disciplina);

        var conseguiuEditar = repositorioMateria?.Editar(materia.Id, materiaEditada);
        dbContext?.SaveChanges();

        // Assert (Asserção)
        var materiaSelecionada = repositorioMateria?.SelecionarRegistroPorId(materia.Id);

        Assert.IsTrue(conseguiuEditar);
        Assert.AreEqual(materia, materiaSelecionada);
    }

    [TestMethod]
    public void Deve_Excluir_Registro_Corretamente()
    {
        // Arrange (Arranjo)
        var disciplina = new Disciplina("Ciências");
        repositorioDisciplina?.Cadastrar(disciplina);

        var materia = new Materia("Geologia", SerieMateria.SegundaSerie, disciplina);
        repositorioMateria?.Cadastrar(materia);

        dbContext?.SaveChanges();

        // Act (Ação)
        var conseguiuExcluir = repositorioMateria?.Excluir(materia.Id);
        dbContext?.SaveChanges();

        // Assert (Asserção)
        var registroSelecionado = repositorioMateria?.SelecionarRegistroPorId(materia.Id);

        Assert.IsTrue(conseguiuExcluir);
        Assert.IsNull(registroSelecionado);
    }

    [TestMethod]
    public void Deve_Selecionar_Registros_Corretamente()
    {
        // Arrange (Arranjo)
        var disciplina = new Disciplina("Biologia");
        var disciplina2 = new Disciplina("Artes");
        var disciplina3 = new Disciplina("Português");

        repositorioDisciplina?.Cadastrar(disciplina);
        repositorioDisciplina?.Cadastrar(disciplina2);
        repositorioDisciplina?.Cadastrar(disciplina3);
        dbContext?.SaveChanges();

        var materia = new Materia("Espécies de Animais", SerieMateria.SegundaSerie, disciplina);
        var materia2 = new Materia("Impressionismo", SerieMateria.PrimeiraSerie, disciplina2);
        var materia3 = new Materia("Conjugação", SerieMateria.SegundaSerie, disciplina3);

        List<Materia> materias = [materia, materia2, materia3];

        repositorioMateria?.Cadastrar(materia);
        repositorioMateria?.Cadastrar(materia2);
        repositorioMateria?.Cadastrar(materia3);
        dbContext?.SaveChanges();

        // Act (Ação)
        var materiasSelecionadas = repositorioMateria?.SelecionarRegistros();

        // Assert (Asserção)
        CollectionAssert.AreEqual(
            materias.OrderBy(m => m.Nome).ToList(),
            materiasSelecionadas?.OrderBy(m => m.Nome).ToList()
        );
    }
}
