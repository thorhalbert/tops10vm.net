using System.IO;
using Thorsbrain.Denc.Language.Element.Programs;

namespace Thorsbrain.Denc.Language.Generator
{
    public class Generate
    {
        private readonly DencProgram dencProg;
        private CodeWriter cw;

        private readonly string[] usings =
            {
                "ThirtySixBits",
                "Thorsbrain.Denc.Runtime",
                "Thorsbrain.Denc.Runtime.Architectures",
                "Thorsbrain.Denc.Runtime.Architectures.PDP10",
            };

        public Generate(DencProgram program)
        {
            dencProg = program;
        }

        public void Setup()
        {
            var flow = new StatementFlowInitializer(dencProg,null);
            foreach (var cl in dencProg.ClassList)
                cl.Setup(dencProg, null, null, ref flow);
        }

        public void Emit(TextWriter tw)
        {
            cw = new CodeWriter(tw);

            cw.WriteHeader(usings, dencProg.ProgNameSpace.ProgNameSpace);

            foreach (var cl in dencProg.ClassList)
                cl.Emit(cw, dencProg);

            cw.Close();
        }
    }
}