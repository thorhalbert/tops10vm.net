using System;
using System.Collections.Generic;
using FileFormats;
using PDP10CPU.BreakPoints;
using PDP10CPU.CPU;
using PDP10CPU.Enums;
using ThirtySixBits;

namespace CommandProcessor
{
    public enum ParserActions
    {
        Ok,
        BadCommand,
        Unimplemented,
        Start,
    }

    public class SimpleParser
    {
        public SimhPDP10CPU CPU { get; set; }

        private enum CmdMap
        {
            None = 0,
            SetCpuType,
            DepSixbit,
            DepAscii,
            DepRad50,
            DepInst,
            Core,
            Start,
            Break,
            Get,
            Dep,
            Examine,
        }

        private readonly List<KeyValuePair<string, CmdMap>> parserValues =
            new List<KeyValuePair<string, CmdMap>>
                {
                    new KeyValuePair<string, CmdMap>("SET CPU/TYPE", CmdMap.SetCpuType),
                    new KeyValuePair<string, CmdMap>("D/SIXBIT", CmdMap.DepSixbit),
                    new KeyValuePair<string, CmdMap>("D/ASCII", CmdMap.DepAscii),
                    new KeyValuePair<string, CmdMap>("D/RAD50", CmdMap.DepRad50),
                    new KeyValuePair<string, CmdMap>("D/INST", CmdMap.DepInst),
                    new KeyValuePair<string, CmdMap>("CORE", CmdMap.Core),
                    new KeyValuePair<string, CmdMap>("START", CmdMap.Start),
                    new KeyValuePair<string, CmdMap>("BREAK", CmdMap.Break),
                    new KeyValuePair<string, CmdMap>("GET", CmdMap.Get),
                    new KeyValuePair<string, CmdMap>("D", CmdMap.Dep),
                    new KeyValuePair<string, CmdMap>("E", CmdMap.Examine),
                };

        public ParserActions Parse(string instring)
        {
            instring = instring.Trim();

            if (instring[0] == '#') return ParserActions.Ok;
            if (instring[0] == ';') return ParserActions.Ok;

            var rstr = "";
            var de = CmdMap.None;
            foreach (var kvp in parserValues)
                if (instring.StartsWith(kvp.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    de = kvp.Value;
                    rstr = instring.Substring(kvp.Key.Length).Trim();
                    break;
                }

            if (de == CmdMap.None) return ParserActions.BadCommand;

            var arr = rstr.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            switch (de)
            {
                case CmdMap.SetCpuType:
                    return procSetCpuType(arr, rstr);
                case CmdMap.DepSixbit:
                    return procDepSixBit(arr, rstr);
                case CmdMap.DepAscii:
                    return procDepAscii(arr, rstr);
                case CmdMap.DepRad50:
                    return ProcRad50(arr, rstr);
                case CmdMap.DepInst:
                    return procInst(arr, rstr);
                case CmdMap.Core:
                    return procCore(arr, rstr);
                case CmdMap.Start:
                    return ProcStart(arr, rstr);
                case CmdMap.Break:
                    return procBreak(arr, rstr);
                case CmdMap.Get:
                    return procGet(arr, rstr);
                case CmdMap.Dep:
                    return procDep(arr, rstr);
                case CmdMap.Examine:
                    return procExamine(arr, rstr);
            }

            return ParserActions.BadCommand;
        }

        private ParserActions procSetCpuType(string[] arr, string rstr)
        {
            if (arr.Length != 1)
                return ParserActions.BadCommand;

            ProcessorTypes cpuT;

            try
            {
                cpuT = (ProcessorTypes) Enum.Parse(typeof (ProcessorTypes), arr[0], true);
            }
            catch (ArgumentException)
            {
                return ParserActions.BadCommand;
            }

            CPU.ProcessorType = cpuT;

            return ParserActions.Ok;
        }

        private static ParserActions procDepSixBit(string[] arr, string rstr)
        {
            return ParserActions.Unimplemented;
        }

        private static ParserActions procDepAscii(string[] arr, string rstr)
        {
            return ParserActions.Unimplemented;
        }

        private ParserActions ProcRad50(string[] arr, string rstr)
        {
            return ParserActions.Unimplemented;
        }

        private static ParserActions procInst(string[] arr, string rstr)
        {
            return ParserActions.Unimplemented;
        }

        private static ParserActions procCore(string[] arr, string rstr)
        {
            return ParserActions.Unimplemented;
        }

        private static ParserActions ProcStart(string[] arr, string rstr)
        {
            return ParserActions.Start;
        }

        private ParserActions procBreak(string[] arr, string rstr)
        {
            if (arr.Length == 1)
            {
                var brval = Convert.ToUInt64(arr[0]).OctUL();
                CPU.BreakPoints.Add(brval, new BreakContext {BreakAction = BreakPointActions.PauseOnValue});
            }

            return ParserActions.Ok;
        }

        private ParserActions procGet(string[] arr, string rstr)
        {
            CPU.CORE.Clear();

            var loader = new Tops10SAVLoader(CPU.CORE, rstr);

            return ParserActions.Ok;
        }

        private static ParserActions procDep(string[] arr, string rstr)
        {
            return ParserActions.Unimplemented;
        }

        private static ParserActions procExamine(string[] arr, string rstr)
        {
            return ParserActions.Unimplemented;
        }
    }
}