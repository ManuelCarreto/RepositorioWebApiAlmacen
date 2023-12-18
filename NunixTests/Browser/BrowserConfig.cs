using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDriverManager.DriverConfigs.Impl;

namespace E2EPruebas.Browser
{
    public class BrowserConfig
    {
        private IWebDriver driver;

        [SetUp]
        public void StartBrowser()
        {
            InitBrowser("Chrome");

            // La espera implícita en Selenium se usa para decirle al controlador web que espere una cierta cantidad de tiempo antes de que arroje una
            // excepción si un elemento no está. También se pueden configurar tiempos de espera explícitos
            // que pausen la prueba y se reanude cuando un elemento cumpla una condición
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            driver.Manage().Window.Maximize();
            driver.Url = "http://localhost:4200/login";
        }

        public void InitBrowser(string browserName)
        {
            switch (browserName)
            {
                case "Firefox":
                    new WebDriverManager.DriverManager().SetUpDriver(new FirefoxConfig());
                    driver = new FirefoxDriver();
                    break;
                case "Chrome":
                    new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
                    driver = new ChromeDriver();
                    break;
                case "Edge":
                    new WebDriverManager.DriverManager().SetUpDriver(new EdgeConfig());
                    driver = new EdgeDriver();
                    break;
            }
        }
        public IWebDriver GetDriver()
        {
            return driver;
        }
    }
}
