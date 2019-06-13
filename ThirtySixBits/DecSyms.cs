using System;
using System.Collections.Generic;
using System.Text;

namespace ThirtySixBits
{
    public static class DECSyms
    {
        public static string ToDEC(string ins)
        {
            ins = ins.Trim();

            var sb = new StringBuilder();
            foreach (var c in ins)
                switch (c)
                {
                    case 'd':
                        sb.Append('$');
                        break;
                    case 'p':
                        sb.Append('%');
                        break;
                    case '_':
                        sb.Append('.');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            return sb.ToString();
        }

        public static string FromDEC(string ins)
        {
            ins = ins.Trim().ToUpperInvariant();

            var sb = new StringBuilder();
            foreach (var c in ins)
                switch (c)
                {
                    case '$':
                        sb.Append('d');
                        break;
                    case '%':
                        sb.Append('p');
                        break;
                    case '.':
                        sb.Append('_');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            return sb.ToString();
        }

        public static List<KeyValuePair<string, T>> MungeEnum<T>(Type iEnum)
        {
            var ret = new List<KeyValuePair<string, T>>();

            var names = Enum.GetNames(iEnum);
            var vals = Enum.GetValues(iEnum);

            for (var i = 0; i < names.Length; i++)
            {
                var nm = ToDEC(names[i]);
                var vl = (T) vals.GetValue(i);

                ret.Add(new KeyValuePair<string, T>(nm, vl));
            }

            return ret;
        }
    }
}