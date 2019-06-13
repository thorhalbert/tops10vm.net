using System;
using System.IO;
using System.Text;

namespace Thorsbrain.Denc.Language.Generator
{
    public class CodeWriter
    {
        private readonly TextWriter writer;

        private int braceLevel;

        public CodeWriter(TextWriter tw)
        {
            writer = tw;
        }

        public int BraceLevel
        {
            get { return braceLevel; }
            set
            {
                if (value > braceLevel + 1)
                    throw new ArgumentException("Cannot Upbrace more than one by setting BraceLevel");

                if (value == braceLevel + 1)
                {
                    UpBrace();
                    return;
                }

                while (BraceLevel > value)
                    DownBrace();
            }
        }

        public void WriteHeader(string[] usings, string namespacename)
        {
            foreach (var v in usings)
                writer.WriteLine("using " + v + ";");

            writer.WriteLine();
            writer.WriteLine("namespace " + namespacename);

            UpBrace();
        }

        public void UpBrace()
        {
            indent();
            writer.WriteLine("{");
            braceLevel++;
        }

        public void DownBrace()
        {
            if (braceLevel <= 0)
                throw new ArgumentException("BraceLevel is already zero -- can't downbrace");

            braceLevel--;
            indent();
            writer.WriteLine("}");
        }

        public void WriteStatement(string statement)
        {
            WriteStatementNoSemi(statement + ";");
        }

        public void WriteStatement(StringBuilder sb)
        {
            WriteStatementNoSemi(sb + ";");
            sb.Clear();
        }
        public void WriteStatementNoSemi(StringBuilder sb)
        {
            indent();
            writer.WriteLine(sb.ToString());

            sb.Clear();
        }

        public void WriteStatementNoSemi(string statement)
        {
            indent();
            writer.WriteLine(statement);
        }

        public void WriteBlank()
        {
            writer.WriteLine();
        }

        public void WriteStatement(string[] statementList)
        {
            foreach (var i in statementList)
                WriteStatement(i);
        }

        public void Close()
        {
            BraceLevel = 0;

            writer.Close();
        }

        private void indent()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < braceLevel; i++)
                sb.Append('\t');

            writer.Write(sb.ToString());
        }
    }
}