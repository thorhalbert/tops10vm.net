using Thorsbrain.Denc.Language.Element.Programs;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.ClassParts
{
    public interface IStorageElement : IStatement
    {
        int Length { get;  }
        int StartAddress { get; set; }
        string Identifier { get; set; }
        bool ExposeProperty { get; set; }
    }
}