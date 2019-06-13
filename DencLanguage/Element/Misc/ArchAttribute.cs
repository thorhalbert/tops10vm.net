using System;
using Thorsbrain.Denc.Language.Architectures;

namespace Thorsbrain.Denc.Language.Element.Misc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ArchAttribute : Attribute
    {
        public ArchitectureTypes Architecture { get; set; }
    }
}