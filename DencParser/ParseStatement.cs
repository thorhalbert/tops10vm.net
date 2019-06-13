using System.Collections.Generic;
using System.IO;
using System.Text;
using Thorsbrain.Denc.Language;

namespace Thorsbrain.Denc.Parser
{
    public class ParseStatement
    {
        private readonly Parser parser;
        private readonly Scanner lexer;

        public ParseStatement(string iString)
        {
            var str = new MemoryStream(Encoding.UTF8.GetBytes(iString));

            lexer = new Scanner(str);
            lexer.YyErrorList.Clear();
            parser = new Parser(lexer);
        }

        public List<YyErrorInstance> GetErrors()
        {
            return lexer.YyErrorList;
        }
    }
}