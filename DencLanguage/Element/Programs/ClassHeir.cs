using System.Collections.Generic;
using System.Text;
using Thorsbrain.Denc.Language.Element.Misc;
using Thorsbrain.Denc.Language.Generator;

namespace Thorsbrain.Denc.Language.Element.Programs
{
    public class ClassHeir : IStatement
    {
        public List<IStatement> StatementList { get; private set; }
        public string ClassName { get; private set; }
        public Protection ClassProtection { get; private set; }
        public string ClassBase { get; private set; }
        public string[] Interfaces { get; private set; }
        public DencProgram Program { get; private set; }

        private readonly List<string> constructorAdd = new List<string>();
        public List<string> ConstructorAdd { get { return constructorAdd;  } }

        public ClassHeir(string val, List<IStatement> statementlist)
        {
            ClassName = val;
            StatementList = statementlist;

            ClassProtection = Protection.Public;
            ClassBase = "";
            Interfaces = new string[0];

          
        }

        public void AddInterface(string interf)
        {
            var intList = new List<string>(Interfaces) {interf};
            Interfaces = intList.ToArray();
        }

        public void Emit(CodeWriter writer, DencProgram program)
        {
            var sb = new StringBuilder();

            var level = writer.BraceLevel;

            var name = program.Architecture.NameClean(ClassName);

            if (ClassBase == "")
                ClassBase = program.ArchitectureElement.ArchType.ToUpperInvariant() + "Class";

            sb.Append(ClassProtection.ToString().ToLowerInvariant());
            sb.Append(" partial class ");
            sb.Append(name);
            sb.Append(" : ");
            sb.Append(ClassBase);

            foreach (var i in Interfaces)
            {
                sb.Append(',');
                sb.Append(i);
            }

            writer.WriteStatementNoSemi(sb);
            writer.UpBrace();

            foreach (var s in StatementList)
                if(s!=null)
                s.Emit(writer, program);

         
            sb.Append("public ");
            sb.Append(name);
            sb.Append("()");
            writer.WriteStatementNoSemi(sb);
            writer.UpBrace();

            foreach (var s in constructorAdd)
                writer.WriteStatement(s);

            writer.DownBrace();

            writer.BraceLevel = level;
        }

     
        public void Setup( DencProgram program,ClassHeir classHeir, IStatement parent, ref StatementFlowInitializer flow)
        {
            Program = program;

            if (classHeir==null)
                    flow = new StatementFlowInitializer(program,this);

            flow.Push();

            foreach (var s in StatementList)
                if (s!=null)
                    s.Setup(program,this, this, ref flow);

            flow.Pop();
        }
    }
}