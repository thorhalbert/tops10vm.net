using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnitTestCPU;

namespace UnitTests.klad
{
    public static class CPURunner
    {
        public static void RunKlad(string CPUType, string diagname, string hash)
        {
            RunKlad(CPUType, diagname, hash, false);
        }

        public static void RunKlad(string CPUType, string diagname, string hash, bool diamon)
        {
            var sb = new StringBuilder();
            sb.Append("SET CPU/TYPE " + CPUType + "\n");
            sb.Append(@"GET ..\..\..\klad\" + diagname + ".sav.asc\n");
            //sb.Append("BREAK 30010");
            sb.Append("START");
            sb.Append("EOF");

            var ba = Encoding.ASCII.GetBytes(sb.ToString());

            var memStr = new MemoryStream(ba);

            var logfile = @"klad\" + diagname + ".log";

            var cmdStream = new StreamReader(memStr);
            var logStream = new StreamWriter(logfile);

            var TestCPU = new MarshallCPU(logStream, cmdStream);
            if (diamon)
                TestCPU.DiaMon = true;

            TestCPU.ProcessCommandFile();

            Console.WriteLine("TestHash=" + TestCPU.ResultHash);

            Assert.AreEqual(hash, TestCPU.ResultHash, "Hash Mismatch");
        }

        public static void Diamon(string type, string s, string s1)
        {
            RunKlad(type, s, s1, true);
        }
    }
}