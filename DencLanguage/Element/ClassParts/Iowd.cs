using System;
using Thorsbrain.Denc.Language.Architectures;
using Thorsbrain.Denc.Language.Element.Misc;
using Thorsbrain.Denc.Language.Element.Programs;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.ClassParts
{
    [Arch(Architecture = ArchitectureTypes.PDP10)]
    internal class Iowd : IStorageElement
    {
        #region Implementation of IStatement

        public void Emit(CodeWriter writer, DencProgram program)
        {
            throw new NotImplementedException();
        }

        public void Setup(DencProgram program, ClassHeir classHeir, IStatement parent, ref StatementFlowInitializer flow)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IStorageElement

        public int Length { get; private set; }
        public int StartAddress { get; set; }
        public string Identifier { get; set; }
        public bool ExposeProperty { get; set; }

        #endregion
    }
}