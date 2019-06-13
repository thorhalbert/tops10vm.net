using System.Text;

namespace ThirtySixBits
{
    /// <summary>
    /// Base 64 works amazingly well for 36 bit encoding since each character is 6 bits...
    /// </summary>
    public static class Base64Cvt
    {
        private static readonly char[] _alphabet =
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G',
                'H', 'I', 'J', 'K', 'L', 'M', 'N',
                'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z',
                'a', 'b', 'c', 'd', 'e', 'f', 'g',
                'h', 'i', 'j', 'k', 'l', 'm', 'n',
                'o', 'p', 'q', 'r', 's', 't', 'u',
                'v', 'w', 'x', 'y', 'z',
                '0', '1', '2', '3', '4', '5',
                '6', '7', '8', '9', '+', '/'
            };

        public static void Append(this StringBuilder sb, Word18 inp)
        {
            var brk = new uint[3];

            var word = inp.UI;

            for (var i = 0; i < 3; i++)
            {
                brk[2 - i] = word & 0x3F;
                word >>= 6;
            }

            for (var i = 0; i < 3; i++)
                sb.Append(_alphabet[brk[i]]);
        }

        public static void Append(this StringBuilder sb, Word36 inp)
        {
            var brk = new uint[6];

            var word = inp.UL;

            for (var i = 0; i < 6; i++)
            {
                brk[5 - i] = (uint) word & 0x3F;
                word >>= 6;
            }

            for (var i = 0; i < 6; i++)
                sb.Append(_alphabet[brk[i]]);
        }

        public static string Convert(Word18 inp)
        {
            var sb = new StringBuilder();
            sb.Append(inp);
            return sb.ToString();
        }

        public static string Convert(Word36 inp)
        {
            var sb = new StringBuilder();
            sb.Append(inp);
            return sb.ToString();
        }
    }
}