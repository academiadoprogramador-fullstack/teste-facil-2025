using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TesteFacil.Testes.Interface.Compartilhado;

namespace TesteFacil.Testes.Interface.ModuloDisciplina;

[TestClass]
[TestCategory("Tests de Interface de Disciplina")]
public sealed class DisciplinaInterfaceTests : TestFixture
{
    [TestMethod]
    public void Deve_Cadastrar_Disciplina_Corretamente()
    {
        // Arrange
        var disciplinaIndex = new DisciplinaIndexPageObject(driver!);

        disciplinaIndex
            .IrPara(enderecoBase);

        // Act
        disciplinaIndex
            .ClickCadastrar()
            .PreencherNome("Matemática")
            .Confirmar();

        // Assert
        Assert.IsTrue(disciplinaIndex.ContemDisciplina("Matemática"));
    }

    [TestMethod]
    public void Deve_Editar_Disciplina_Corretamente()
    {
        // Arrange
        var disciplinaIndex = new DisciplinaIndexPageObject(driver!);

        disciplinaIndex
            .IrPara(enderecoBase)
            .ClickCadastrar()
            .PreencherNome("Matemática")
            .Confirmar();

        // Act
        disciplinaIndex
            .ClickEditar()
            .PreencherNome("Matemática Editada")
            .Confirmar();

        // Assert
        Assert.IsTrue(disciplinaIndex.ContemDisciplina("Matemática Editada"));
    }

    [TestMethod]
    public void Deve_Excluir_Disciplina_Corretamente()
    {
        // Arrange
        var disciplinaIndex = new DisciplinaIndexPageObject(driver!);

        disciplinaIndex
            .IrPara(enderecoBase)
            .ClickCadastrar()
            .PreencherNome("Matemática")
            .Confirmar();

        // Act
        disciplinaIndex
            .ClickExcluir()
            .ConfirmarExclusao();

        // Assert
        Assert.IsFalse(disciplinaIndex.ContemDisciplina("Matemática"));
    }
}
