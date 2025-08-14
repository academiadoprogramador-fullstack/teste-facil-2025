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
        
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        wait.Until(d => d.FindElement(By.CssSelector("form")).Displayed);
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
        wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']"))).Click();        

        //esperar carregar um elemento na página...
        wait.Until(d => d.FindElement(By.CssSelector("a[data-se='btnCadastrar']")).Displayed);

        return new DisciplinaIndexPageObject(driver!);
    }

}