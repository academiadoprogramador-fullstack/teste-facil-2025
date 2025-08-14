using TesteFacil.Testes.Interface.Compartilhado;

namespace TesteFacil.Testes.Interface.ModuloDisciplina;

[TestClass]
[TestCategory("Tests de Interface de Disciplina")]
public sealed class DisciplinaInterfaceTests : TestFixture
{
    [TestMethod]
    [Retry(3, MillisecondsDelayBetweenRetries = 500)]
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
        Assert.AreEqual(1, indexPageObject.ContarDisciplinas());
    }

    [TestMethod]
    [Retry(3, MillisecondsDelayBetweenRetries = 500)]
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
    [Retry(3, MillisecondsDelayBetweenRetries = 500)]
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
            .ConfirmarExclusao();

        // Assert
        Assert.IsFalse(indexPageObject.ContemDisciplina("Matemática"));
    }
}
