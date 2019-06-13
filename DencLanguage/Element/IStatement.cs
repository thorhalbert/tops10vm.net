using Thorsbrain.Denc.Language.Element.Programs;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element
{
    public interface IStatement
    {
        void Emit(CodeWriter writer, DencProgram program);
        void Setup(DencProgram program, ClassHeir classHeir, IStatement parent, ref StatementFlowInitializer flow);
    }
}