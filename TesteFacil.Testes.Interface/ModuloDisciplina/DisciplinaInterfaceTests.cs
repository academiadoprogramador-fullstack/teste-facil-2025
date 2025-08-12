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
        // Arange
        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas"));

        var elemento = driver?.FindElement(By.CssSelector("a[data-se='btnCadastrar']"));

        elemento?.Click();

        // Act
        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        var elementosCard = driver?.FindElements(By.CssSelector(".card"));

        Assert.AreEqual(1, elementosCard?.Count);
    }

    [TestMethod]
    public void Deve_Editar_Disciplina_Corretamente()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        wait.Until(d => d.FindElement(By.CssSelector("form")));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElement(By.CssSelector(".card a[title='Edição']"))).Click();

        // Act
        driver?.FindElement(By.Id("Nome")).SendKeys(" Editada");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        wait.Until(d => d.PageSource.Contains("Matemática Editada"));

        Assert.IsTrue(driver?.PageSource.Contains("Matemática Editada"));
    }

    [TestMethod]
    public void Deve_Excluir_Disciplina_Corretamente()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        wait.Until(d => d.FindElement(By.CssSelector("form")));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElement(By.CssSelector(".card a[title='Exclusão']"))).Click();

        // Act
        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        wait.Until(d => !d.PageSource.Contains("Matemática"));

        Assert.IsFalse(driver?.PageSource.Contains("Matemática"));
    }
}
