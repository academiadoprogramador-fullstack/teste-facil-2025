using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TesteFacil.Testes.Interface.ModuloDisciplina;

public class DisciplinaFormPageObject
{
    private readonly IWebDriver driver;
    private readonly WebDriverWait wait;

    public DisciplinaFormPageObject(IWebDriver driver)
    {
        this.driver = driver;

        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElement(By.CssSelector("form")).Displayed);
    }

    public DisciplinaFormPageObject PreencherNome(string nome)
    {
        var nomeInput = driver.FindElement(By.Id("Nome"));
        nomeInput.Clear();
        nomeInput.SendKeys(nome);
        return this;
    }

    public DisciplinaIndexPageObject Confirmar()
    {
        driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        return new DisciplinaIndexPageObject(driver);
    }
}
