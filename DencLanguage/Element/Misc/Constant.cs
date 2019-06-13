using System;
using Thorsbrain.Denc.Language.Element.Programs;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.Misc
{
    public class Constant : IStatement
    {
        public Constant(string val)
        {
            Value = val;
        }

        public string Value { get; set; }

        private int radix=8;

        public int Number(int overrideRadix)
        {
            switch (overrideRadix)
            {
                case 2:
                case 8:
                case 10:
                case 16:
                    return Convert.ToInt32(Value, overrideRadix);
                default:
                    throw new ApplicationException("Unsupported Radix " + overrideRadix);
            }
        }

        public int Number()
        {
            return Number(radix);
        }

        #region Implementation of IStatement

        public void Emit(CodeWriter writer, DencProgram program)
        {
        }

        public void Setup(DencProgram program, ClassHeir classHeir, IStatement parent, ref StatementFlowInitializer flow)
        {
            radix = flow.Radix;
        }

        #endregion
    }
}