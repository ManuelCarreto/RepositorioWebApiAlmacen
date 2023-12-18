using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDriverManager.DriverConfigs.Impl;

namespace E2EPruebas
{
    public class TestLogin
    {
        IWebDriver driver;

        [SetUp]

        public void Setup()
        {
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();

            // La espera implícita en Selenium se usa para decirle al controlador web que espere una cierta cantidad de tiempo antes de que arroje una
            // excepción sin un elemento no está. También se pueden configurar tiempos de espera explícitos
            // que pausen la prueba y se reanude cuando un elemente cumpla una condición
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            driver.Manage().Window.Maximize();
            driver.Url = "http://localhost:4200/login";
            // driver.Url = "http://localhost:63418/login";
        }

        [Test]
        public void Test()
        {
            driver.FindElement(By.Id("usuario")).Clear();
            driver.FindElement(By.Id("usuario")).SendKeys("paco@paco.com");
            driver.FindElement(By.Name("password")).Clear();
            driver.FindElement(By.Name("password")).SendKeys("paco");
            // driver.FindElement(By.ClassName("btn-lg")).Click();
            IWebElement button = driver.FindElement(By.ClassName("btn-lg"));
            button.Click();
        }
    }
}
