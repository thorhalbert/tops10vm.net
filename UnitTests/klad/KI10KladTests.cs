using NUnit.Framework;

namespace UnitTests.klad
{
    [TestFixture]
    public class KI10KladTests
    {
        private const string CPUType = "KI10";

        [Test(Description = "PDP-10 KI10 BASIC INSTRUCTION DIAGNOSTIC (1) [DBKAA]")]
        public void DBKAA()
        {
            CPURunner.RunKlad(CPUType, "dbkaa", "9807E5FCA9C50A2C1C3E2DDD5B5AACD6FDBF128A");
        }

        [Test(Description = "PDP-10 KI10 BASIC INSTRUCTION DIAGNOSTIC (2) [DBKAB]")]
        public void DBKAB()
        {
            CPURunner.RunKlad(CPUType, "dbkab", "D32BE82BDE31431026DB3AD89C83F4A06599335E");
        }

        [Test(Description = "PDP-10 KI10 BASIC INSTRUCTION DIAGNOSTIC (4) [DBKAD]")]
        public void DBKAD()
        {
            CPURunner.RunKlad(CPUType, "dbkad", "97A85CED05BD254416F30D63B0FAADF6E083A85F");
        }

        [Test(Description = "PDP-10 KI10 BASIC INSTRUCTION DIAGNOSTIC (5) [DBKAE]")]
        public void DBKAE()
        {
            CPURunner.RunKlad(CPUType, "dbkae", "EA97B2B49358A3714884B4E67196C1F5699D09D5");
        }

        [Test(Description = "PDP-10 KI10 BASIC INSTRUCTION DIAGNOSTIC (6) [DBKAF]")]
        public void DBKAF()
        {
            CPURunner.RunKlad(CPUType, "dbkaf", "9F67A8BBAF920892F526EE623148D67100E8EE6E");
        }

        [Test(Description = "PDP-10 KI10 BASIC INSTRUCTION DIAGNOSTIC (7) [DBKAG]")]
        public void DBKAG()
        {
            CPURunner.RunKlad(CPUType, "dbkag", "DFA4D88F1A53F852D42C4C41AC4E710BC4CB02C2");
        }

        // Unfortunately - does kernel I/O
        //[Test(Description = "DIAG")]
        //[Ignore("?Instruction Failure: procMUUO unimplemented [PC:00030710/700040,,000001<Opcode 448> EA:000001] [EOF]")]
        //public void DBKAH()
        //{
        //    CPURunner.RunKlad(CPUType, "dbkah", "a");
        //}

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKAI()
        {
            CPURunner.Diamon(CPUType, "dbkai", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKAJ()
        {
            CPURunner.Diamon(CPUType, "dbkaj", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKAK()
        {
            CPURunner.Diamon(CPUType, "dbkak", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKAL()
        {
            CPURunner.Diamon(CPUType, "dbkal", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKAM()
        {
            CPURunner.Diamon(CPUType, "dbkam", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKBA()
        {
            CPURunner.Diamon(CPUType, "dbkba", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKBB()
        {
            CPURunner.Diamon(CPUType, "dbkbb", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKBC()
        {
            CPURunner.Diamon(CPUType, "dbkbc", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKBD()
        {
            CPURunner.Diamon(CPUType, "dbkbd", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKCA()
        {
            CPURunner.Diamon(CPUType, "dbkca", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKCC()
        {
            CPURunner.Diamon(CPUType, "dbkcc", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKCD()
        {
            CPURunner.Diamon(CPUType, "dbkcd", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKCE()
        {
            CPURunner.Diamon(CPUType, "dbkce", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKDA()
        {
            CPURunner.Diamon(CPUType, "dbkda", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DBKDB()
        {
            CPURunner.Diamon(CPUType, "dbkdb", "a");
        }
    }
}