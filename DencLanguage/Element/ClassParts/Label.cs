using System;
using Thorsbrain.Denc.Language.Element.Programs;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.ClassParts
{
    public class Label : IStatement
    {
        public string Name { get; private set; }

        public Label(string val)
        {
            Name = val;
        }

        #region Implementation of IStatement

        public void Emit(CodeWriter writer, DencProgram program)
        {
         
        }

        public void Setup(DencProgram program, ClassHeir classHeir, IStatement parent, ref StatementFlowInitializer flow)
        {
            flow.LastLabel = this;

        }

        #endregion
    }
}