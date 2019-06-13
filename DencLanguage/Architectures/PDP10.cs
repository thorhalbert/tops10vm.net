using System.Text;
using Thorsbrain.Denc.Language.Element.Programs;

namespace Thorsbrain.Denc.Language.Architectures
{
    public class PDP10 : ArchitectureBase
    {
        public PDP10(DencProgram program)
        {
            Program = program;
            ArchitectureType = ArchitectureTypes.PDP10;
            DefaultRadix = 8;
        }

        /// <summary>
        /// Make PDP10 acceptable names - includes ., $, and %
        /// These get mapped to more acceptable modern characters (in lower case).
        /// The normal letters are always in upper case.
        /// </summary>
        /// <param name="inname"></param>
        /// <returns></returns>
        public override string NameClean(string inname)
        {
            var sb = new StringBuilder();

            var vNam = inname.ToUpperInvariant().Trim();

            foreach (var c in vNam)
            {
                var i = c;

                if (c == '.') i = '_';
                if (c == '$') i = 'd';
                if (c == '%') i = 'p';

                sb.Append(i);
            }

            return sb.ToString();
        }
    }
}