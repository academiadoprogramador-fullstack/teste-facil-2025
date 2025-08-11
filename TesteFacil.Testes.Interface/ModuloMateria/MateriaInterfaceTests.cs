using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TesteFacil.Testes.Interface.Compartilhado;

namespace TesteFacil.Testes.Interface.ModuloMateria;

[TestClass]
[TestCategory("Testes de Interface de Matéria")]
public class MateriaInterfaceTests : TestFixture
{
    [TestMethod]
    public void Deve_Cadastrar_Materia()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type=submit]")).Click();
        
        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "materias", "cadastrar"));

        // Act
        driver?.FindElement(By.Id("Nome")).SendKeys("Quatro Operações");

        var select = new SelectElement(driver?.FindElement(By.Id("DisciplinaId"))!);

        select.SelectByText("Matemática");

        driver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        // Assert
        var elementosCard = driver?.FindElements(By.CssSelector(".card"));

        Assert.AreEqual(1, elementosCard?.Count);
    }
}
