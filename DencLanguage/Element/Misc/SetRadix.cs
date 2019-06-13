using Thorsbrain.Denc.Language.Element.Programs;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.Misc
{
    public class SetRadix : IStatement
    {
        public Constant BaseSet { get; private set; }

        public SetRadix(Constant val)
        {
            BaseSet = val;
        }

        #region Implementation of IStatement

        public void Emit(CodeWriter writer, DencProgram program)
        {
           
        }

        public void Setup(DencProgram program, ClassHeir classHeir, IStatement parent, ref StatementFlowInitializer flow)
        {
            flow.Radix = BaseSet.Number(10); // Radixen are always base 10
        }

        #endregion
    }
}