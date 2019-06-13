using NUnit.Framework;

namespace UnitTests.klad
{
    [TestFixture]
    [Ignore("Nothing Loads")]
    public class KL10KladTests
    {
        private const string CPUType = "KL10";

        [Test(Description = "DIAG")]
        public void DFKAA()
        {
            CPURunner.RunKlad(CPUType, "dfkaa", "F55443B9188866A689D9D192E70B72B26C072A0D");
        }

        [Test(Description = "DIAG")]
        public void DFKAD()
        {
            CPURunner.Diamon(CPUType, "dfkad", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKBB()
        {
            CPURunner.Diamon(CPUType, "dfkbb", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKCA()
        {
            CPURunner.Diamon(CPUType, "dfkca", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKCB()
        {
            CPURunner.Diamon(CPUType, "dfkcb", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKCC()
        {
            CPURunner.Diamon(CPUType, "dfkcc", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKDA()
        {
            CPURunner.Diamon(CPUType, "dfkda", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKEA()
        {
            CPURunner.Diamon(CPUType, "dfkea", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKEB()
        {
            CPURunner.RunKlad(CPUType, "dfkeb", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKEC()
        {
            CPURunner.Diamon(CPUType, "dfkec", "a");
        }

        [Test(Description = "DIAG")]
        public void DFKED()
        {
            CPURunner.Diamon(CPUType, "dfked", "a");
        }
    }
}