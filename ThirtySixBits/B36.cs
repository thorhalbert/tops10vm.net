using System;
using System.Collections.Generic;
using System.Text;

namespace ThirtySixBits
{
    public static class B36
    {
        public const int VASIZE = 18; // virtual addr width 
        //private static readonly ulong AMASK = ((VASIZE.bit()) - 1); // virtual addr mask 
        public const ulong AMASK = RMASK; // virtual addr mask 
        public const ulong LMASK = 0xFFFFC0000ul; // left mask 
        public const ulong LSIGN = 0x800000000ul; // left sign 
        public const ulong RMASK = 0x00003FFFFul; // right mask 
        public const ulong RSIGN = 0x000020000ul; // data mask 
        public const ulong DMASK = 0xFFFFFFFFFul; // 0777777777777 data mask 
        public const ulong SIGN = 0x800000000ul; // 0400000000000 sign 
        public const ulong MMASK = 0x7FFFFFFFFul; // 0377777777777 magnitude mask 
        public const ulong ONES = 0xFFFFFFFFFul; // 0777777777777
        public const ulong MAXPOS = 0x7FFFFFFFFul; // 0377777777777;
        public const ulong MAXNEG = 0x800000000ul; // 0400000000000;

        // Thor's porting helpers

        public static UInt64 Bit(this int x)
        {
            UInt64 con = 1;
            con <<= x;

            return con;
        }

        public static ulong OctUL(this ulong x)
        {
            ulong mult = 1;
            ulong sum = 0;

            while (x != 0)
            {
                sum += (x%10)*mult;
                x /= 10;
                mult *= 8;
            }

            return sum;
        }

        public static Word36 Oct36(this ulong x)
        {
            return new Word36(OctUL(x));
        }

        public static int Oct(this ulong x)
        {
            return Convert.ToInt32(OctUL(x));
        }

        public static int Oct(this int x)
        {
            return Convert.ToInt32(OctUL(Convert.ToUInt64(x)));
        }

        public static uint OctU(this ulong x)
        {
            return Convert.ToUInt32(OctUL(x));
        }

        public static uint OctU(this int x)
        {
            return Convert.ToUInt32(Oct(x));
        }

        public static Word18 Oct18(this int x)
        {
            return new Word18(OctU(x));
        }

        public static string ToOctal(this int word)
        {
            return ToOctal(word, 0);
        }

        public static string ToOctal(this int word, int prtdigits)
        {
            return ToOctal((ulong) word, prtdigits);
        }

        public static string ToOctal(this uint word)
        {
            return ToOctal(word, 0);
        }

        public static string ToOctal(this uint word, int prtdigits)
        {
            return ToOctal((ulong) word, prtdigits);
        }

        public static string ToOctal(this ulong word, int prtdigits)
        {
            if (prtdigits < 1)
                prtdigits = 1;

            var digits = new Stack<char>();
            while (word > 0)
            {
                var b = word%8;
                digits.Push(Convert.ToChar(b + 48));
                word /= 8;
            }

            while (digits.Count < prtdigits)
                digits.Push('0');

            var sb = new StringBuilder(digits.Count);
            while (digits.Count > 0)
                sb.Append(digits.Pop());

            return sb.ToString();
        }

        public static bool NZ(this ulong x)
        {
            return x != 0;
        }

        public static bool NZ(this int x)
        {
            return x != 0;
        }

        public static bool Z(this ulong x)
        {
            return x == 0;
        }

        public static bool Z(this int x)
        {
            return x == 0;
        }

        public static ulong B(this bool x)
        {
            return x ? 1UL : 0;
        }

        public static ulong RWD(this ulong x)
        {
            return (((x) & RMASK));
        }

        public static uint RWDI(this ulong x)
        {
            var i = (((x) & RMASK));
            return Convert.ToUInt32(i);
        }

        public static ulong LWD(this ulong x)
        {
            return (((x >> 18) & RMASK));
        }

        public static uint LWDI(this ulong x)
        {
            var i = (((x >> 18) & RMASK));
            return Convert.ToUInt32(i);
        }

        public static ulong XWD(this ulong x)
        {
            var r = x.RWD();
            var l = x.LWD();

            return l << 18 | r;
        }

        public static Int32 SignE(this ulong x)
        {
            var b = BitConverter.GetBytes(x);
            return BitConverter.ToInt32(b, 0);
        }

        public static int Slice(this ulong value, int bits, ulong mask)
        {
            return Convert.ToInt32((value >> bits) & mask);
        }

        public static void SetSlice(ref ulong baseInt, int bits, ulong mask, int value)
        {
            // Clean the value
            var v = Convert.ToUInt64(value);
            v &= mask;

            // Mask out the hole the value goes in
            var pMask = mask << bits;
            baseInt &= (~pMask);

            // Move the value into place and put on
            var nv = v << bits;
            baseInt |= nv;
        }
    }
}