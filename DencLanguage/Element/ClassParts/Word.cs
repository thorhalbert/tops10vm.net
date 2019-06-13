using System;
using System.Text;
using Thorsbrain.Denc.Language.Architectures;
using Thorsbrain.Denc.Language.Element.Misc;
using Thorsbrain.Denc.Language.Element.Programs;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.ClassParts
{
    [Arch(Architecture = ArchitectureTypes.All)]
    public class Word : IStorageElement
    {
        private ClassHeir parent;
        public Constant Wordsize { get; private set; }

        public Word(Constant constant)
        {
            Wordsize = constant;
        }

        #region Implementation of IStatement

        public void Emit(CodeWriter writer, DencProgram program)
        {
            if (Length>1)
            {
                EmitArray(writer, program);
                return;
            }
            var sb = new StringBuilder();

            sb.Append("private Word _");
            sb.Append(Identifier);

            writer.WriteStatement(sb);

            sb.Append('_');
            sb.Append(Identifier);
            sb.Append(" = new Word(this, ");
            sb.Append(StartAddress.ToString());
           sb.Append(")");

            parent.ConstructorAdd.Add(sb.ToString());
            sb.Clear();

            if (ExposeProperty)
            {
                sb.Append("public Word36 ");
                sb.Append(Identifier);
                sb.Append("{ get { return ");
                sb.Append('_');
                sb.Append(Identifier);
                sb.Append(".GetValue; } set { ");
                sb.Append('_');
                sb.Append(Identifier);
                sb.Append(".SetValue(value); } }");

                writer.WriteStatementNoSemi(sb);
            }
        }

        private void EmitArray(CodeWriter writer, DencProgram program)
        {
            var sb = new StringBuilder();

            sb.Append("private Words _");
            sb.Append(Identifier);

            writer.WriteStatement(sb);

            sb.Append('_');
            sb.Append(Identifier);
            sb.Append(" = new Words(this, ");
            sb.Append(StartAddress.ToString());
            sb.Append(", ");
            sb.Append(Length.ToString());
            sb.Append(")");

            parent.ConstructorAdd.Add(sb.ToString());
            sb.Clear();

            if (ExposeProperty)
            {
                sb.Append("public Words ");
                sb.Append(Identifier);
                sb.Append("{ get { return ");
                sb.Append('_');
                sb.Append(Identifier);
                sb.Append("; } }");
              
                writer.WriteStatementNoSemi(sb);
            }
        }

        public void Setup(DencProgram program, ClassHeir classHeir, IStatement sparent,
                          ref StatementFlowInitializer flow)
        {
            parent = classHeir;
            Length = Wordsize.Number();
            flow.SetupStorage(this);
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