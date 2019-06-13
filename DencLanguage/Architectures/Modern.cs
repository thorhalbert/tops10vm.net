using Thorsbrain.Denc.Language.Element.Programs;

namespace Thorsbrain.Denc.Language.Architectures
{
    public class Modern : ArchitectureBase
    {
        public Modern(DencProgram program)
        {
            Program = program;
            ArchitectureType = ArchitectureTypes.Modern;
            DefaultRadix = 10;
        }
    }
}