using TesteFacil.Testes.Interface.Compartilhado;

namespace TesteFacil.Testes.Interface.ModuloDisciplina;

[TestClass]
[TestCategory("Tests de Interface de Disciplina")]
public sealed class DisciplinaInterfaceTests : TestFixture
{
    [TestMethod]
    public void Deve_Cadastrar_Disciplina_Corretamente()
    {
        // Arange
        var indexPageObject = new DisciplinaIndexPageObject(driver!)
            .IrPara(enderecoBase!);

        // Act
        indexPageObject
            .ClickCadastrar()
            .PreencherNome("Matemática")
            .Confirmar();

        // Assert
        Assert.IsTrue(indexPageObject.ContemDisciplina("Matemática"));
    }

    [TestMethod]
    public void Deve_Editar_Disciplina_Corretamente()
    {
        // Arrange
        var indexPageObject = new DisciplinaIndexPageObject(driver!)
            .IrPara(enderecoBase!);

        indexPageObject
            .ClickCadastrar()
            .PreencherNome("Matemática")
            .Confirmar();

        // Act
        indexPageObject
            .ClickEditar()
            .PreencherNome("Matemática Editada")
            .Confirmar();

        // Assert
        Assert.IsTrue(indexPageObject.ContemDisciplina("Matemática Editada"));
    }

    [TestMethod]
    public void Deve_Excluir_Disciplina_Corretamente()
    {
        // Arrange
        var indexPageObject = new DisciplinaIndexPageObject(driver!)
            .IrPara(enderecoBase!);

        indexPageObject
            .ClickCadastrar()
            .PreencherNome("Matemática")
            .Confirmar();

        // Act
        indexPageObject
            .ClickExcluir()
            .Confirmar();

        // Assert
        Assert.IsFalse(indexPageObject.ContemDisciplina("Matemática"));
    }
}
