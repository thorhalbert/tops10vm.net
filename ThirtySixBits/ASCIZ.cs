using System;
using System.Collections.Generic;

namespace ThirtySixBits
{
    public class ASCIZ
    {
        public static IEnumerable<char> Get(Word36 val)
        {
            var ret = new char[5];

            var v = val.UL;
            v >>= 1; // Strip off the rightmost bit

            for (var i = 0; i < 5; i++)
            {
                ret[4 - i] = Convert.ToChar(v & 127ul);
                v >>= 7;
            }

            return ret;
        }
    }
}