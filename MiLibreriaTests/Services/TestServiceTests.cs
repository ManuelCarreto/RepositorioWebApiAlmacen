using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiLibreria.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiLibreria.Services.Tests
{
    [TestClass()]
    public class TestServiceTests
    {
        [TestMethod()]
        public void CalcularIvaTest()
        {
            var testService = new TestService();
            var result = testService.CalcularIva(10);
            Assert.AreEqual(0.4m, result);
        }

        [TestMethod()]
        public void CalcularDescuentoTest()
        {
            var testService = new TestService(); 
            var result = testService.CalcularDescuento(10, 50);
            Assert.AreEqual(5m, result);
        }
    }
}