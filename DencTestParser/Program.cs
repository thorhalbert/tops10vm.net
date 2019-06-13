using System.IO;
using Thorsbrain.Denc.Language.Generator;
using Thorsbrain.Denc.Parser;

namespace DencTestParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = new DoParser(new FileStream(@"..\..\test1.denc", FileMode.Open));
            var output = parser.Parse();
            var errors = parser.GetErrors();

            if (output != null)
            {
                var outF = new StreamWriter(@"..\..\..\DencTestRuntime\test1.gen.cs", false);

                var gen = new Generate(output);
                gen.Setup();
                gen.Emit(outF);
            }
        }
    }
}