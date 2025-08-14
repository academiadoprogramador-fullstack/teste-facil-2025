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

        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public MateriaIndexPageObject IrPara(string enderecoBase)
    {
        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "materias"));

        return this;
    }

    public MateriaFormPageObject ClickCadastrar()
    {
        wait.Until(d => d.FindElement(By.CssSelector("a[data-se='btnCadastrar']"))).Click();

        return new MateriaFormPageObject(driver!);
    }

    public MateriaFormPageObject ClickEditar()
    {
        wait.Until(d => d?.FindElement(By.CssSelector(".card a[title='Edição']"))).Click();

        return new MateriaFormPageObject(driver!);
    }

    public MateriaFormPageObject ClickExcluir()
    {
        wait.Until(d => d?.FindElement(By.CssSelector(".card a[title='Exclusão']"))).Click();

        return new MateriaFormPageObject(driver!);
    }

    public bool ContemMateria(string nome)
    {
        return driver.PageSource.Contains(nome);
    }
}