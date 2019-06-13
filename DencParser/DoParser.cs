using System.Collections.Generic;
using System.IO;
using System.Text;
using Thorsbrain.Denc.Language;
using Thorsbrain.Denc.Language.Element.Programs;

namespace Thorsbrain.Denc.Parser
{
    public class DoParser
    {
        private Scanner lexer;
        private Parser parser;

        public DoParser(string iString)
        {
            var str = new MemoryStream(Encoding.UTF8.GetBytes(iString));

            preProcess(str);
        }

        public DoParser(Stream str)
        {
            preProcess(str);
        }

        private void preProcess(Stream instr)
        {
            var ins = new StreamReader(instr);
            var sb = new StringBuilder();

            var ot = 0;

            while (!ins.EndOfStream)
            {
                var r = ins.ReadLine();
                if (r.Trim().Length <= 0)
                {
                    sb.AppendLine();
                    continue;
                }

                var tc = 0;

                while (tc < r.Length &&
                       r[tc] == '\t') tc++;

                while (tc > ot)
                {
                    sb.Append("@@OPEN@@ ");
                    ot++;
                }

                while (tc < ot)
                {
                    sb.Append("@@CLOSE@@ ");
                    ot--;
                }

                sb.AppendLine(r.Trim());
            }

            while (ot > 0)
            {
                sb.Append("@@CLOSE@@ ");
                ot--;
            }

            ins.Close();

            var str = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

            SetupParser(str);
        }

        private void SetupParser(Stream str)
        {
            lexer = new Scanner(str);
            lexer.YyErrorList.Clear();
            parser = new Parser(lexer);
        }

        public DencProgram Parse()
        {
            return !parser.Parse() ? null : parser.DencMainProgram;
        }

        public List<YyErrorInstance> GetErrors()
        {
            return lexer.YyErrorList;
        }
    }
}