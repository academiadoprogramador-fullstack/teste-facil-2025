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
    }

    public DisciplinaFormPageObject PreencherNome(string nome)
    {
        var inputNome = driver?.FindElement(By.Id("Nome"));
        inputNome?.Clear();
        inputNome?.SendKeys(nome);

        return this;
    }

    public DisciplinaIndexPageObject Confirmar()
    {
        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 1);

        return new DisciplinaIndexPageObject(driver!);
    }

    public DisciplinaIndexPageObject ConfirmarExclusao()
    {
        driver?.FindElement(By.CssSelector("button[type='submit']")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".card")).Count == 0);

        return new DisciplinaIndexPageObject(driver!);
    }
}