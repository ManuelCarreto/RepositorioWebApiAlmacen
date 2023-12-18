using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E2EPruebas.PageSpecs
{
    public class LoginPage
    {
        private IWebDriver driver;

        public LoginPage(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }
        // Elementos de la página

        [FindsBy(How = How.Id, Using = "usuario")]
        private IWebElement usuario;

        [FindsBy(How = How.Name, Using = "password")]
        private IWebElement password;

        [FindsBy(How = How.ClassName, Using = "btn-lg")]
        private IWebElement loginButton;

        public IWebElement GetUsuario()
        {
            return usuario;
        }

        public IWebElement GetPassword()
        {
            return password;
        }
        public void ValidLogin(string user, string pass)
        {
            usuario.Clear();
            password.Clear();
            usuario.SendKeys(user);
            password.SendKeys(pass);
            loginButton.Click();
        }
    }
}
