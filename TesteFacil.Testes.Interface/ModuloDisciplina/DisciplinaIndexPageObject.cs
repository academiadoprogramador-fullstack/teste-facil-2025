using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TesteFacil.Testes.Interface.ModuloDisciplina;

public class DisciplinaIndexPageObject
{
    private readonly IWebDriver driver;
    private readonly WebDriverWait wait;

    public DisciplinaIndexPageObject(IWebDriver driver)
    {
        this.driver = driver;
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
    }

    public DisciplinaIndexPageObject IrPara(string baseUrl)
    {
        driver.Navigate().GoToUrl(Path.Combine(baseUrl, "disciplinas"));

        return this;
    }

    public DisciplinaFormPageObject ClickCadastrar()
    {
        driver.FindElement(By.CssSelector("a[data-se='btnCadastrar']")).Click();

        return new DisciplinaFormPageObject(driver);
    }

    public DisciplinaFormPageObject ClickEditar()
    {
        wait.Until(d =>d.FindElement(By.CssSelector(".card a[title='Edição']"))).Click();

        return new DisciplinaFormPageObject(driver);
    }

    public DisciplinaIndexPageObject ClickExcluir()
    {
        wait.Until(d => d.FindElement(By.CssSelector(".card a[title='Exclusão']"))).Click();

        return this;
    }

    public DisciplinaIndexPageObject ConfirmarExclusao()
    {
        wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']"))).Click();

        return this;
    }

    public bool ContemDisciplina(string nome) =>
        driver.PageSource.Contains(nome);

    public int ContarDisciplinas() =>
        driver.FindElements(By.CssSelector(".card")).Count;
}