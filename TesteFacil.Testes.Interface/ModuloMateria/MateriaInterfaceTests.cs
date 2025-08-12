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

        wait.Until(d => d.FindElement(By.CssSelector("form")));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "materias", "cadastrar"));

        wait.Until(d => d.FindElement(By.CssSelector("form")));

        // Act
        driver?.FindElement(By.Id("Nome")).SendKeys("Quatro Operações");

        var selectSerie = new SelectElement(driver?.FindElement(By.Id("Serie"))!);

        selectSerie.SelectByText("Segunda Série");

        var selectDisciplina = new SelectElement(driver?.FindElement(By.Id("DisciplinaId"))!);

        selectDisciplina.SelectByText("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        wait.Until(d => d.PageSource.Contains("Quatro Operações"));

        Assert.IsTrue(driver?.PageSource.Contains("Quatro Operações"));
    }

    [TestMethod]
    public void Deve_Editar_Materia_Corretamente()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        wait.Until(d => d.FindElement(By.CssSelector("form")));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "materias", "cadastrar"));

        driver?.FindElement(By.Id("Nome")).SendKeys("Quatro Operações");

        new SelectElement(driver?.FindElement(By.Id("Serie"))!).SelectByText("Segunda Série");

        new SelectElement(driver?.FindElement(By.Id("DisciplinaId"))!).SelectByText("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElement(By.CssSelector(".card a[title='Edição']"))).Click();

        // Act
        wait.Until(d => d.FindElement(By.CssSelector("form")));

        driver?.FindElement(By.Id("Nome")).SendKeys(" Editada");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        wait.Until(d => d.PageSource.Contains("Quatro Operações Editada"));

        Assert.IsTrue(driver?.PageSource.Contains("Quatro Operações Editada"));
    }

    [TestMethod]
    public void Deve_Excluir_Materia_Corretamente()
    {
        // Arrange
        var wait = new WebDriverWait(driver!, TimeSpan.FromSeconds(5));

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas", "cadastrar"));

        wait.Until(d => d.FindElement(By.CssSelector("form")));

        driver?.FindElement(By.Id("Nome")).SendKeys("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "materias", "cadastrar"));

        driver?.FindElement(By.Id("Nome")).SendKeys("Quatro Operações");

        new SelectElement(driver?.FindElement(By.Id("Serie"))!).SelectByText("Segunda Série");

        new SelectElement(driver?.FindElement(By.Id("DisciplinaId"))!).SelectByText("Matemática");

        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElement(By.CssSelector(".card a[title='Exclusão']"))).Click();

        // Act
        wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']"))).Click();

        // Assert
        wait.Until(d => !d.PageSource.Contains("Quatro Operações"));

        Assert.IsFalse(driver?.PageSource.Contains("Quatro Operações"));
    }
}
