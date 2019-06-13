using QUT.Gppg;
using Thorsbrain.Denc.Language.Element.Programs;

namespace Thorsbrain.Denc.Parser
{
    internal partial class Parser
    {
        public DencProgram DencMainProgram { get; private set; }

        public Parser(AbstractScanner<ValueType, LexLocation> scanner) : base(scanner) {}
    }
}