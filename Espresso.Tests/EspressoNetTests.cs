using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Espresso.Tests
{

    [TestClass]
    public class EspressoNetTests
    {

        public TestContext TestContext { get; set; }

        void TestFile(string file)
        {
            var pla = PLA.Parse(new StreamReader(File.OpenRead(file)));
            TestContext.WriteLine(pla.ToString());
            TestContext.WriteLine("Cubes: {0}", pla.Cover.Count);
            var cov = new PLA(EspressoNet.Espresso(pla.Cover, pla.CoverType));
            TestContext.WriteLine(cov.ToString());
            TestContext.WriteLine("Cubes: {0}", cov.Cover.Count);
        }

        [TestMethod]
        public void Test_examples_alu1()
        {
            TestFile(@"examples\alu1");
        }

        [TestMethod]
        public void Test_hard_examples_ex1010()
        {
            TestFile(@"hard_examples\ex1010");
        }

    }

}
