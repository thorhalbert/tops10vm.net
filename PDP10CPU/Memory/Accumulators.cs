using System;
using ThirtySixBits;

namespace PDP10CPU.Memory
{
    public class Accumulators
    {
        private readonly UserModeCore mainCore;

        public ulong this[int loc]
        {
            get { return this[(ulong) loc]; }
            set { this[(ulong) loc] = value; }
        }

        public ulong this[ulong loc]
        {
            get
            {
                checkAc((int) loc);
                return mainCore[0, loc].UL;
            }
            set
            {
                checkAc((int) loc);
                mainCore[0, loc] = new Word36(value);
            }
        }

        private static void checkAc(int loc)
        {
            if (loc < 0 || loc > 15) // Can also do this with an and
                throw new Exception("Accumulator Access was out of range");
        }

        public Accumulators(UserModeCore mainMem)
        {
            mainCore = mainMem;
        }
    }
}