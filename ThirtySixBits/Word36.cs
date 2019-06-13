using System;
using PDP10CPU;

namespace ThirtySixBits
{
    public struct Word36 : IEquatable<Word18>,
                           IEquatable<Word36>
    {
        // Version 1, store in an int64

        private UInt64 baseInt;

        public Word36(Word36 initial)
        {
            baseInt = initial.baseInt & B36.DMASK;
        }

        public Word36(UInt32 initial)
        {
            baseInt = initial & B36.DMASK;
        }

        public Word36(UInt64 initial)
        {
            baseInt = initial & B36.DMASK;
        }

        public Word36(Word18 lw, Word18 rw)
        {
            baseInt = (lw.UL << 18) | rw.UL;
        }

        public Word36(int ld, int rd) : this(Convert.ToUInt32(ld), Convert.ToUInt32(rd)) {}

        public Word36(uint lw, uint rw)
        {
            baseInt = lw & B36.RMASK;
            baseInt <<= 18;
            baseInt |= rw & B36.RMASK;
        }

        //public static UInt36[] BytePack(int byteSize, params int[] bytes)
        //{
        //    return new UInt36[0];
        //}

        public ulong UL
        {
            get { return baseInt & B36.DMASK; }
            set { baseInt = (value & B36.DMASK); }
        }

        public Word18 LHW
        {
            get
            {
                var lt = baseInt >> 18;
                lt &= B36.RMASK;
                return new Word18(lt);
            }
            set
            {
                baseInt &= B36.RMASK;
                baseInt |= (value.UL << 18);
            }
        }

        public Word18 RHW
        {
            get
            {
                var rt = baseInt & B36.RMASK;
                return new Word18(rt);
            }
            set
            {
                baseInt &= B36.LMASK;
                baseInt |= value.UL;
            }
        }

        public Word36 XWD
        {
            get { return new Word36(RHW.UI, LHW.UI); }
        }

        private const int InstVOp = 27; // opcode 
        private static readonly ulong _instMOp = B36.OctUL(0777);
        private const int InstVDev = 26;
        private static readonly ulong _instMDev = B36.OctUL(0177); // device 
        private const int InstVac = 23; // AC 
        private static readonly ulong _instMac = B36.OctUL(017);
        private const int InstVInd = 22; // indirect 
        private static readonly ulong _instMInd = InstVInd.Bit();
        private const int InstVXr = 18; // index 
        private static readonly ulong _instMXr = B36.OctUL(017);

        public OpCodes OPCODE
        {
            get { return (OpCodes) baseInt.Slice(InstVOp, _instMOp); }
            set { B36.SetSlice(ref baseInt, InstVOp, _instMOp, (int) value); }
        }

        public int DEVICE
        {
            get { return baseInt.Slice(InstVDev, _instMDev); }
            set { B36.SetSlice(ref baseInt, InstVDev, _instMDev, value); }
        }

        public int AC
        {
            get { return baseInt.Slice(InstVac, _instMac); }
            set { B36.SetSlice(ref baseInt, InstVac, _instMac, value); }
        }

        public bool IND
        {
            get { return (baseInt & _instMInd) != 0; }
            set
            {
                if (value)
                    baseInt |= _instMInd;
                else
                    baseInt &= ~_instMInd;
            }
        }

        public int XR
        {
            get { return baseInt.Slice(InstVXr, _instMXr); }
            set { B36.SetSlice(ref baseInt, InstVXr, _instMXr, value); }
        }

        public bool Z
        {
            get { return baseInt == 0; }
        }

        public static bool operator true(Word36 arg)
        {
            return arg.Z;
        }

        public bool NZ
        {
            get { return baseInt != 0; }
        }

        public static bool operator false(Word36 arg)
        {
            return arg.NZ;
        }

        public static Word36 operator &(Word36 arg1, Word36 arg2)
        {
            var ans = arg1.baseInt & arg2.baseInt;
            ans &= B36.DMASK;

            return new Word36(ans);
        }

        public static Word36 operator |(Word36 arg1, Word36 arg2)
        {
            var ans = arg1.baseInt | arg2.baseInt;
            ans &= B36.DMASK;

            return new Word36(ans);
        }

        public static Word36 operator +(Word36 arg1, Word36 arg2)
        {
            var ans = arg1.baseInt + arg2.baseInt;
            ans &= B36.DMASK;

            return new Word36(ans);
        }

        public static Word36 operator -(Word36 arg1, Word36 arg2)
        {
            var ans = arg1.baseInt - arg2.baseInt;
            ans &= B36.DMASK;

            return new Word36(ans);
        }

        public static explicit operator Word36(int inv)
        {
            return new Word36(Convert.ToUInt64(inv));
        }

        public static explicit operator Word36(uint inv)
        {
            return new Word36(Convert.ToUInt64(inv));
        }

        public static explicit operator Word36(Word18 inv)
        {
            return new Word36(inv.UL);
        }

        public static explicit operator Word36(UInt64 inv)
        {
            return new Word36(inv);
        }

        public bool Equals(Word18 other)
        {
            return LHW.Z &&
                   RHW.Equals(other);
        }

        public bool Equals(Word36 other)
        {
            return other.baseInt == baseInt;
        }

        public override string ToString()
        {
            return LHW + ",," + RHW;
        }
    }
}