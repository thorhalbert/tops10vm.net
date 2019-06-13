using System;
using System.Collections.Generic;
using Thorsbrain.Denc.Language;

namespace Thorsbrain.Denc.Parser
{
    internal sealed partial class Scanner
    {
        public readonly List<YyErrorInstance> YyErrorList = new List<YyErrorInstance>();

        public override void yyerror(string format, params object[] args)
        {
            YyErrorList.Add(new YyErrorInstance
                                {
                                    StartLine = yylloc.StartLine,
                                    StartColumn = yylloc.StartColumn,
                                    EndLine = yylloc.EndLine,
                                    EndColumn = yylloc.EndColumn,
                                    Error = string.Format(format, args),
                                });

            Console.Write("[{0}:{1}/{2}:{3}] ",
                          yylloc.StartLine,
                          yylloc.StartColumn,
                          yylloc.EndLine,
                          yylloc.EndColumn);
            Console.WriteLine(string.Format(format, args));
        }

        private static int CheckType(string s)
        {
            switch (s)
            {
                case ".":
                    return (int) Token.PERIOD;
                case "new":
                    return (int) Token.NEW;
                case "out":
                    return (int) Token.OUT;
                case "ref":
                    return (int) Token.REF;
                case "true":
                    return (int) Token.TRUE;
                case "false":
                    return (int) Token.FALSE;
                default:
                    return (int) Token.IDENTIFIER;
            }
        }
    }
}