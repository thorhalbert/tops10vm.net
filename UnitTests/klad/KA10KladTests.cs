using NUnit.Framework;

namespace UnitTests.klad
{
    [TestFixture]
    public class KA10KladTests
    {
        private const string CPUType = "KA10";

        [Test(Description = "PDP-10 KA10 BASIC INSTRUCTION DIAGNOSTIC (1) [DAKAA]")]
        public void DAKAA()
        {
            CPURunner.RunKlad(CPUType, "dakaa", "F55443B9188866A689D9D192E70B72B26C072A0D");
        }

        [Test(Description = "PDP-10 KA10 BASIC INSTRUCTION DIAGNOSTIC (2) [DAKAB]")]
        public void DAKAB()
        {
            CPURunner.RunKlad(CPUType, "dakab", "2798D210FB0A754E7C87CC4D47BF860E68750946");
        }

        [Test(Description = "PDP-10 KA10 BASIC INSTRUCTION DIAGNOSTIC (3) [DAKAC]")]
        public void DAKAC()
        {
            CPURunner.RunKlad(CPUType, "dakac", "E38BE174A01451ECD6D85D52C6B9631D05D16AD8");
        }

        [Test(Description = "PDP-10 KA10 BASIC INSTRUCTION DIAGNOSTIC (4) [DAKAD]")]
        public void DAKAD()
        {
            CPURunner.RunKlad(CPUType, "dakad", "B6B03F3A8B6E051077AB73B355F810EE7438D473");
        }

        [Test(Description = "PDP-10 KA10 BASIC INSTRUCTION DIAGNOSTIC (5) [DAKAE]")]
        public void DAKAE()
        {
            CPURunner.RunKlad(CPUType, "dakae", "FB4CEDD5784E1CD2647577552FAD956FEA44568E");
        }

        [Test(Description = "PDP-10 KA10 BASIC INSTRUCTION DIAGNOSTIC (6) [DAKAF]")]
        public void DAKAF()
        {
            CPURunner.RunKlad(CPUType, "dakaf", "C9177FFFC495FE97880FF22D7CACBC04BE43D751");
        }

        [Test(Description = "PDP-10 KI10 BASIC INSTRUCTION DIAGNOSTIC (7) [DAKAG]")]
        [Ignore("?Instruction Failure: procMUUO unimplemented [PC:00031240/254200,,031240<Opcode JRST> EA:031240] [EOF]")]
        public void DAKAG()
        {
            CPURunner.RunKlad(CPUType, "dakag", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("?EXEC MODE DIAGNOSTIC ONLY")]
        public void DAKAH()
        {
            CPURunner.RunKlad(CPUType, "dakah", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKAI()
        {
            CPURunner.Diamon(CPUType, "dakai", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKAJ()
        {
            CPURunner.Diamon(CPUType, "dakaj", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKAK()
        {
            CPURunner.Diamon(CPUType, "dakak", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKAL()
        {
            CPURunner.Diamon(CPUType, "dakal", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKAM()
        {
            CPURunner.Diamon(CPUType, "dakam", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKBA()
        {
            CPURunner.Diamon(CPUType, "dakba", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKBB()
        {
            CPURunner.Diamon(CPUType, "dakbb", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKBC()
        {
            CPURunner.Diamon(CPUType, "dakbc", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKBD()
        {
            CPURunner.Diamon(CPUType, "dakbd", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKCA()
        {
            CPURunner.Diamon(CPUType, "dakca", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKCB()
        {
            CPURunner.Diamon(CPUType, "dakcb", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKDA()
        {
            CPURunner.Diamon(CPUType, "dakda", "a");
        }

        [Test(Description = "DIAG")]
        [Ignore("Need diamon to run")]
        public void DAKDB()
        {
            CPURunner.Diamon(CPUType, "dakdb", "a");
        }
    }
}