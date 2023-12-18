using E2EPruebas.Browser;
using E2EPruebas.PageSpecs;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E2EPruebas.Tests
{
    public class E2ELoginYEditoriales : BrowserConfig
    {
        [Test]
        public void Test()
        {
            //driver.FindElement(By.Id("usuario")).Clear();
            //driver.FindElement(By.Id("usuario")).SendKeys("jl@hotmail.com");
            //driver.FindElement(By.Name("password")).Clear();
            //driver.FindElement(By.Name("password")).SendKeys("123456");
            LoginPage loginPage = new LoginPage(GetDriver());
            //loginPage.getUsuario().Clear();
            //loginPage.getUsuario().SendKeys("string"); //son credenciales del back
            //loginPage.getPassword().Clear();
            //loginPage.getPassword().SendKeys("string"); //son credenciales del back
            loginPage.ValidLogin("string", "string");
        }

        [TearDown]
        public void AfterTest()
        {
            GetDriver().Quit();
        }
    }
}
