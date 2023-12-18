using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E2EPruebas
{
    public class PruebaCicloVidaTest
    {
        [SetUp]
        public void Setup()
        {
            TestContext.Progress.WriteLine("Ejecutado método setup");
        }

        [Test]
        public void Test1()
        {
            TestContext.Progress.WriteLine("Ejecutado test 1");
        }

        [Test]
        public void Test2()
        {
            TestContext.Progress.WriteLine("Ejecutado test 2");
        }

        [TearDown]
        public void EndTest()
        {
            TestContext.Progress.WriteLine("Test finalizado (TearDown)");
        }
    }
}
