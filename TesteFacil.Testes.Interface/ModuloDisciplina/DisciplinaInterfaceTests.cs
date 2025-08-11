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
        webDriver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        webDriver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        // Act
        webDriver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        // Assert
        var elementosCard = webDriver?.FindElements(By.CssSelector(".card"));

        Assert.AreEqual(1, elementosCard?.Count);
    }

    [TestMethod]
    public void Deve_Editar_Disciplina()
    {
        // Arrange
        var wait = new WebDriverWait(webDriver!, TimeSpan.FromSeconds(5));

        webDriver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        webDriver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        webDriver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        webDriver?.FindElement(By.CssSelector(".card"))
            .FindElement(By.CssSelector("a[title='Edição']")).Click();

        webDriver?.FindElement(By.Id("Nome")).SendKeys(" Editada");

        // Act
        webDriver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        // Assert
        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        Assert.IsTrue(webDriver?.PageSource.Contains("Matemática Editada"));
    }

    [TestMethod]
    public void Deve_Excluir_Disciplina()
    {
        // Arrange
        var wait = new WebDriverWait(webDriver!, TimeSpan.FromSeconds(5));

        webDriver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        webDriver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        webDriver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        webDriver?.FindElement(By.CssSelector(".card"))
            .FindElement(By.CssSelector("a[title='Exclusão']")).Click();

        // Act
        webDriver?.FindElement(By.CssSelector("button[type=submit]")).Click();

        // Assert
        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 0);

        var elementosCard = webDriver?.FindElements(By.CssSelector(".card"));

        Assert.AreEqual(0, elementosCard?.Count);
    }
}
