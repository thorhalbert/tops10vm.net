using NUnit.Framework;

namespace UnitTests.klad
{
    [TestFixture]
    public class KS10KladTests
    {
        private const string CPUType = "KS10";

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (1) [DSKAA]")]
        public void DSKAA()
        {
            CPURunner.RunKlad(CPUType, "dskaa", "96961C2B8ED1D611FC12330DBCAA59143D152F3C");
        }

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (2) [DSKAB]")]
        public void DSKAB()
        {
            CPURunner.RunKlad(CPUType, "dskab", "FDFF876BA9A73E8D52BF825CE1649B0E600BC11D");
        }

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (3) [DSKAC]")]
        public void DSKAC()
        {
            CPURunner.RunKlad(CPUType, "dskac", "0934E1B1D5D73C3D41E8C26CAA9000109B654E18");
        }

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (4) [DSKAD]")]
        public void DSKAD()
        {
            CPURunner.RunKlad(CPUType, "dskad", "80866BDAA52C3403C5F868470457A61639F85EAE");
        }

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (5) [DSKAE]")]
        public void DSKAE()
        {
            CPURunner.RunKlad(CPUType, "dskae", "4F257E9CBF67A6F387D74C7D53F05B208E83E9C6");
        }

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (6) [DSKAF]")]
        public void DSKAF()
        {
            CPURunner.RunKlad(CPUType, "dskaf", "84B144A9A70852F57A201B129879554F34B58795");
        }

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (7) [DSKAG]")]
        public void DSKAG()
        {
            CPURunner.RunKlad(CPUType, "dskag", "772BCF7AD0783F800EAE155926221419BE45C4F0");
        }

        [Test(Description = "DECSYSTEM-2020 BASIC INSTRUCTION DIAGNOSTIC (8) [DSKAH]")]
        public void DSKAH()
        {
            CPURunner.RunKlad(CPUType, "dskah", "0E78F7A7A3251F6F38A992FDE2666FF3CEC658BE");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKAI()
        {
            CPURunner.Diamon(CPUType, "dskai", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKAJ()
        {
            CPURunner.Diamon(CPUType, "dskaj", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKAK()
        {
            CPURunner.Diamon(CPUType, "dskak", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKAL()
        {
            CPURunner.Diamon(CPUType, "dskal", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKAM()
        {
            CPURunner.Diamon(CPUType, "dskam", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKBA()
        {
            CPURunner.Diamon(CPUType, "dskba", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKCA()
        {
            CPURunner.Diamon(CPUType, "dskca", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKCB()
        {
            CPURunner.Diamon(CPUType, "dskcb", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKCC()
        {
            CPURunner.Diamon(CPUType, "dskcc", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKCD()
        {
            CPURunner.Diamon(CPUType, "dskcd", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKCE()
        {
            CPURunner.Diamon(CPUType, "dskce", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKCF()
        {
            CPURunner.Diamon(CPUType, "dskcf", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKCG()
        {
            CPURunner.Diamon(CPUType, "dskcg", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKDA()
        {
            CPURunner.Diamon(CPUType, "dskda", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKEA()
        {
            CPURunner.Diamon(CPUType, "dskea", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKEB()
        {
            CPURunner.Diamon(CPUType, "dskeb", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKEC()
        {
            CPURunner.Diamon(CPUType, "dskec", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DSKFA()
        {
            CPURunner.Diamon(CPUType, "dskfa", "a");
        }
    }
}