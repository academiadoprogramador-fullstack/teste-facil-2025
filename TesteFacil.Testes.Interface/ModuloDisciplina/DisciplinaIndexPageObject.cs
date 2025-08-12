using OpenQA.Selenium;

namespace TesteFacil.Testes.Interface.ModuloDisciplina;

public class DisciplinaIndexPageObject
{
    private readonly IWebDriver driver;

    public DisciplinaIndexPageObject(IWebDriver driver)
    {
        this.driver = driver;
    }

    public DisciplinaIndexPageObject IrPara(string enderecoBase)
    {
        driver?.Navigate().GoToUrl(Path.Combine(enderecoBase, "disciplinas"));

        return this;
    }

    public DisciplinaFormPageObject ClickCadastrar()
    {
        driver?.FindElement(By.CssSelector("a[data-se='btnCadastrar']")).Click();

        return new DisciplinaFormPageObject(driver!);
    }

    public DisciplinaFormPageObject ClickEditar()
    {
        driver?.FindElement(By.CssSelector(".card a[title='Edição']")).Click();

        return new DisciplinaFormPageObject(driver!);
    }

    public DisciplinaFormPageObject ClickExcluir()
    {
        driver?.FindElement(By.CssSelector(".card a[title='Exclusão']")).Click();

        return new DisciplinaFormPageObject(driver!);
    }

    public bool ContemDisciplina(string nome)
    {
        return driver.PageSource.Contains(nome);
    }
}