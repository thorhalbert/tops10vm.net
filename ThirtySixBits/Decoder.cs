using System;
using System.Text;
using PDP10CPU;
using Symbols;

namespace ThirtySixBits
{
    public static class Decoder
    {
        public static string Instruction(this ulong x)
        {
            return Instruction(new Word36(x));
        }

        public static string Instruction(this Word36 word36)
        {
            return Instruction(word36, 0);
        }

        public static string Instruction(this Word36 w, int pc)
        {
            // Need to implement extended JRST, CALLI, JSYS, TTCALL, and MTAPE macros

            var sb = new StringBuilder();

            var ins = w.OPCODE.ToString();
            if (ins.StartsWith("MUUO") ||
                ins.StartsWith("LUUO")) return "";

            var ac = w.AC;
            var xr = w.XR;
            var addr = w.RHW.UL;

            switch (w.OPCODE)
            {
                case OpCodes.JRST:
                    var jrstop = (JRST) ac;
                    ins = jrstop.ToString();
                    ac = 0;
                    break;
                case OpCodes.JFCL:
                    var jfcl = (JFCL) ac;
                    ins = jfcl.ToString();
                    ac = 0;
                    break;
                case OpCodes.CALLI:
                    var calli = (CALLIs) addr;
                    ins = calli.ToString();
                    addr = 0;
                    break;
                case OpCodes.JSYS:
                    var jsys = (JSYSs) addr;
                    ins = jsys.ToString();
                    addr = 0;
                    break;
                case OpCodes.TTCALL:
                    var ttcal = (TTCALLs) ac;
                    ins = ttcal.ToString();
                    ac = 0;
                    break;
                case OpCodes.MTAPE:
                    var mtape = (MTAPEs) addr;
                    ins = mtape.ToString();
                    addr = 0;
                    break;
            }

            sb.Append(DECSyms.ToDEC(ins));
            sb.Append(' ');
            if (ac > 0)
            {
                sb.Append(ac.ToOctal(1));
                sb.Append(", ");
            }
            if (w.IND)
                sb.Append("@");
            if (addr > 0)
            {
                var adt = (int) addr;

                var tmpaddr = adt - pc;
                if (Math.Abs(tmpaddr) > 16)
                    sb.Append(addr.ToOctal(1));
                else
                {
                    sb.Append('.');
                    if (tmpaddr > 0)
                        sb.Append('+');
                    sb.Append(tmpaddr);
                }
            }
            if (xr > 0)
            {
                sb.Append('(');
                sb.Append(xr.ToOctal(1));
                sb.Append(')');
            }

            return sb.ToString();
        }

        public static string ASCII(this Word36 w)
        {
            var sb = new StringBuilder();

            var v = w.UL;
            v >>= 1; // Strip off the rightmost bit

            for (var i = 0; i < 5; i++)
            {
                var b = Convert.ToChar(v & 127ul);
                if (Char.IsControl(b)) b = '.';
                sb.Insert(0, b);
                v >>= 7;
            }

            return sb.ToString();
        }

        public static string SIXBIT(this Word36 w)
        {
            var sb = new StringBuilder();

            var v = w.UL;

            for (var i = 0; i < 6; i++)
            {
                var b = Convert.ToChar((v & 63ul) + 32);
                sb.Insert(0, b);

                v >>= 6;
            }

            return sb.ToString();
        }
    }
}