using Thorsbrain.Denc.Language.Element.Programs;

namespace Thorsbrain.Denc.Language.Architectures
{
    public abstract class ArchitectureBase
    {
        public DencProgram Program { get; protected set; }
        public ArchitectureTypes ArchitectureType { get; protected set; }
        public int DefaultRadix { get; protected set; }

        public virtual string NameClean(string inname)
        {
            return inname.Trim();
        }
    }
}