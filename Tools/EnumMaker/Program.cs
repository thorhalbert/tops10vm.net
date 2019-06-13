using System;
using System.IO;
using ThirtySixBits;

namespace EnumMaker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var inf = new StreamReader(args[0]);
            var ouf = new StreamWriter(args[1]);

            while (!inf.EndOfStream)
            {
                var lin = inf.ReadLine();
                var parts = lin.Split('|');

                var v = Convert.ToUInt64(parts[1]);
                var h = v.OctUL();

                var name = parts[0];
                name = DECSyms.FromDEC(parts[0]);
                var com = "";
                if (parts.Length > 2)
                    com = parts[2];

                ouf.WriteLine(name + " =\t0x" + h.ToString("x") + ",  // [" + h.ToOctal(3) + "] " + com);
            }

            ouf.Close();
            inf.Close();
        }
    }
}