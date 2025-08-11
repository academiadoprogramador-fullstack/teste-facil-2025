using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TesteFacil.Testes.Interface.Compartilhado;

namespace TesteFacil.Testes.Interface.ModuloMateria;

[TestClass]
[TestCategory("Testes de Interface de Matéria")]
public sealed class MateriaInterfaceTests : TestFixture
{
    [TestMethod]
    public void Deve_Cadastrar_Materia_Corretamente()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "materias", "cadastrar"));

        // Act
        driver?.FindElement(By.Id("Nome")).SendKeys("Quatro Operações");

        var selectSerie = new SelectElement(driver?.FindElement(By.Id("Serie"))!);

        selectSerie.SelectByText("Segunda Série");

        var selectDisciplina = new SelectElement(driver?.FindElement(By.Id("DisciplinaId"))!);

        selectDisciplina.SelectByText("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        var elementosCard = driver?.FindElements(By.CssSelector(".card"));

        Assert.AreEqual(1, elementosCard?.Count);
    }
}
