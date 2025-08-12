using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TesteFacil.Testes.Interface.ModuloMateria;

public class MateriaIndexPageObject
{
    private readonly IWebDriver driver;
    private readonly WebDriverWait wait;

    public MateriaIndexPageObject(IWebDriver driver)
    {
        this.driver = driver;

        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
    }

    public MateriaIndexPageObject IrPara(string baseUrl)
    {
        driver.Navigate().GoToUrl(Path.Combine(baseUrl, "materias"));

        return this;
    }

    public MateriaFormPageObject ClickCadastrar()
    {
        driver.FindElement(By.CssSelector("a[data-se='btnCadastrar']")).Click();

        return new MateriaFormPageObject(driver);
    }

    public MateriaFormPageObject ClickEditar()
    {
        wait.Until(d => d.FindElement(By.CssSelector(".card a[title='Edição']"))).Click();

        return new MateriaFormPageObject(driver);
    }

    public MateriaIndexPageObject ClickExcluir()
    {
        wait.Until(d => d.FindElement(By.CssSelector(".card a[title='Exclusão']"))).Click();

        return this;
    }

    public MateriaIndexPageObject ConfirmarExclusao()
    {
        wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']"))).Click();

        return this;
    }

    public bool ContemMateria(string nomeMateria) =>
        driver.PageSource.Contains(nomeMateria);

    public int ContarMaterias() =>
        driver.FindElements(By.CssSelector(".card")).Count;
}