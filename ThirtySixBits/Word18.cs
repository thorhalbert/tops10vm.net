using System;

namespace ThirtySixBits
{
    public struct Word18 : IEquatable<Word18>
    {
        public const uint RMASK = 0x3FFFFu; // right mask 

        private UInt32 baseInt;

        public ulong UL
        {
            get { return baseInt & RMASK; }
            set { baseInt = Convert.ToUInt32(value & RMASK); }
        }

        public uint UI
        {
            get { return baseInt & RMASK; }
            set { baseInt = value & RMASK; }
        }

        public Word18(UInt32 v)
        {
            baseInt = v & RMASK;
        }

        public Word18(Word36 v)
        {
            baseInt = Convert.ToUInt32(v.UL);
        }

        public Word18(UInt64 v)
        {
            var s = v & RMASK;
            baseInt = Convert.ToUInt32(s);
        }

        public bool Z
        {
            get { return baseInt == 0; }
        }

        public static bool operator true(Word18 arg)
        {
            return arg.Z;
        }

        public static bool operator <(Word18 arg, Word18 arg2)
        {
            var lh = arg.baseInt & RMASK;
            var rh = arg2.baseInt & RMASK; 

            return lh < rh;
        }

        public static bool operator >(Word18 arg, Word18 arg2)
        {
            var lh = arg.baseInt & RMASK;
            var rh = arg2.baseInt & RMASK;

            return lh > rh;
        }

        public static bool operator <=(Word18 arg, Word18 arg2)
        {
            var lh = arg.baseInt & RMASK;
            var rh = arg2.baseInt & RMASK;

            return lh <= rh;
        }

        public static bool operator >=(Word18 arg, Word18 arg2)
        {
            var lh = arg.baseInt & RMASK;
            var rh = arg2.baseInt & RMASK;

            return lh >= rh;
        }

        public bool NZ
        {
            get { return baseInt != 0; }
        }

        public static bool operator false(Word18 arg)
        {
            return arg.NZ;
        }

        public static Word18 operator &(Word18 arg1, Word18 arg2)
        {
            var ans = arg1.baseInt & arg2.baseInt;
            ans &= RMASK;

            return new Word18(ans);
        }

        public static Word18 operator |(Word18 arg1, Word18 arg2)
        {
            var ans = arg1.baseInt | arg2.baseInt;
            ans &= RMASK;

            return new Word18(ans);
        }

        public static Word18 operator +(Word18 arg1, Word18 arg2)
        {
            var ans = arg1.baseInt + arg2.baseInt;
            ans &= RMASK;

            return new Word18(ans);
        }

        public static Word18 operator -(Word18 arg1, Word18 arg2)
        {
            var ans = arg1.baseInt - arg2.baseInt;
            ans &= RMASK;

            return new Word18(ans);
        }

        public static explicit operator Word18(int inv)
        {
            return new Word18(Convert.ToUInt64(inv));
        }

        public static explicit operator Word18(uint inv)
        {
            var v = Convert.ToUInt64(inv);
            v &= RMASK;
            return new Word18(Convert.ToUInt32(v));
        }

        public static explicit operator Word18(UInt64 inv)
        {
            return new Word18(inv);
        }

        public bool Equals(Word18 other)
        {
            return baseInt == other.baseInt;
        }

        public override string ToString()
        {
            return baseInt.ToOctal(6);
        }
    }
}