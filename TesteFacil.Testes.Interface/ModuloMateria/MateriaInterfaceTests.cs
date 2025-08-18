using TesteFacil.Testes.Interface.Compartilhado;
using TesteFacil.Testes.Interface.ModuloDisciplina;

namespace TesteFacil.Testes.Interface.ModuloMateria;

[TestClass]
[TestCategory("Testes de Interface de Matéria")]
public sealed class MateriaInterfaceTests : TestFixture
{
    [TestMethod]
    public void Deve_Cadastrar_Materia_Corretamente()
    {
        // Arrange
        new DisciplinaIndexPageObject(driver!)
           .IrPara(enderecoBase!)
           .ClickCadastrar()
           .PreencherNome("Matemática")
           .Confirmar();

        // Act
        var materiaIndex = new MateriaIndexPageObject(driver!)
           .IrPara(enderecoBase!);

        materiaIndex
            .ClickCadastrar()
            .PreencherNome("Quatro Operações")
            .SelecionarSerie("Segunda Série")
            .SelecionarDisciplina("Matemática")
            .Confirmar();

        // Assert
        Assert.IsTrue(materiaIndex.ContemMateria("Quatro Operações"));
    }

    [TestMethod]
    public void Deve_Editar_Materia_Corretamente()
    {
        // Arrange
        new DisciplinaIndexPageObject(driver!)
           .IrPara(enderecoBase!)
           .ClickCadastrar()
           .PreencherNome("Matemática")
           .Confirmar();

        var materiaIndex = new MateriaIndexPageObject(driver!)
            .IrPara(enderecoBase!);

        materiaIndex
            .ClickCadastrar()
            .PreencherNome("Quatro Operações")
            .SelecionarSerie("Segunda Série")
            .SelecionarDisciplina("Matemática")
            .Confirmar();

        // Act
        materiaIndex
            .ClickEditar()
            .PreencherNome("Quatro Operações Editada")
            .SelecionarSerie("Primeira Série")
            .SelecionarDisciplina("Matemática")
            .Confirmar();

        // Assert
        Assert.IsTrue(materiaIndex.ContemMateria("Quatro Operações Editada"));
    }

    [TestMethod]
    public void Deve_Excluir_Materia_Corretamente()
    {
        // Arrange
        new DisciplinaIndexPageObject(driver!)
            .IrPara(enderecoBase!)
            .ClickCadastrar()
            .PreencherNome("Matemática")
            .Confirmar();

        var materiaIndex = new MateriaIndexPageObject(driver!)
            .IrPara(enderecoBase!);

        materiaIndex
            .ClickCadastrar()
            .PreencherNome("Quatro Operações")
            .SelecionarSerie("Segunda Série")
            .SelecionarDisciplina("Matemática")
            .Confirmar();

        // Act
        materiaIndex
            .ClickExcluir()
            .Confirmar();

        // Assert
        Assert.IsFalse(materiaIndex.ContemMateria("Quatro Operações"));
    }
}
