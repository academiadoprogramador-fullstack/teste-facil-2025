using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TesteFacil.Testes.Interface.Compartilhado;

namespace TesteFacil.Testes.Interface.ModuloDisciplina;

[TestClass]
[TestCategory("Testes de Interface de Disciplina")]
public sealed class DisciplinaInterfaceTests : TestFixture
{
    [TestMethod]
    public void Deve_Cadastrar_Disciplina()
    {
        // Arrange
        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        // Act
        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        // Assert
        var elementosCard = driver?.FindElements(By.CssSelector(".card"));

        Assert.AreEqual(1, elementosCard?.Count);
    }

    [TestMethod]
    public void Deve_Editar_Disciplina()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        driver?.FindElement(By.CssSelector(".card"))
            .FindElement(By.CssSelector("a[title='Edição']")).Click();

        // Act
        driver?.FindElement(By.Id("Nome")).SendKeys(" Editada");

        driver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        // Assert
        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        Assert.IsTrue(driver?.PageSource.Contains("Matemática Editada"));
    }

    [TestMethod]
    public void Deve_Excluir_Disciplina()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        driver?.FindElement(By.CssSelector(".card"))
            .FindElement(By.CssSelector("a[title='Exclusão']")).Click();

        // Act
        driver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        // Assert
        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 0);

        var elementosCard = driver?.FindElements(By.CssSelector(".card"));

        Assert.AreEqual(0, elementosCard?.Count);
    }
}
