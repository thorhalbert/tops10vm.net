using System;
using System.IO;
using System.Text;
using Monitor.Subsystems.Console;
using Monitor.Subsystems.UUOHandlers;
using PDP10CPU;
using PDP10CPU.CPU;
using PDP10CPU.Enums;
using PDP10CPU.Exceptions;
using Symbols;
using ThirtySixBits;

namespace Monitor.Subsystems
{
    public class TTCALL : IUUOHandler
    {
        public event EventHandler<ConsoleOutputEvent> ConsoleOutput;

        private MonitorContext Monitor;

        private readonly StreamWriter consoleOutputStream = new StreamWriter(new ConsoleStreamWriter(), Encoding.ASCII);

        public StreamWriter ConsoleOutputStream
        {
            get { return consoleOutputStream; }
        }

        public void Setup(MonitorContext ctx)
        {
            Monitor = ctx;
        }

        public void AttachToConsole()
        {
            consoleOutputStream.NewLine = "\r\n";

            //Console.SetOut(consoleOutputStream);
            //Console.SetError(consoleOutputStream);
        }

        public InstructionExit HandleUUO(SimhPDP10CPU processor, ulong instruction,
                                         OpCodes opcode, int ac, ulong ea)
        {
            switch ((TTCALLs) ac)
            {
                case TTCALLs.OUTSTR: // [TTCALL 3,] OUTPUT STRING
                    OutputASCIZ(processor, ea);
                    break;

                case TTCALLs.INCHRW: // [TTCALL 0,] INPUT CHAR AND WAIT
                case TTCALLs.OUTCHR: // [TTCALL 1,] OUTPUT CHAR
                case TTCALLs.INCHRS: // [TTCALL 2,] INPUT CHAR AND SKIP
                case TTCALLs.INCHWL: // [TTCALL 4,] INPUT CHAR WAIT, LINE
                case TTCALLs.INCHSL: // [TTCALL 5,] INPUT CHAR SKIP, LINE
                case TTCALLs.GETLCH: // [TTCALL 6,] GET LINE CHARS
                case TTCALLs.SETLCH: // [TTCALL 7,] SET LINE CHARS
                case TTCALLs.RESCAN: // [TTCALL 10,] RESET INPUT LINE
                case TTCALLs.CLRBFI: // [TTCALL 11,] CLEAR INPUT BUFFER
                case TTCALLs.CLRBFO: // [TTCALL 12,] CLEAR OUTPUT BUFFER
                case TTCALLs.SKPINC: // [TTCALL 13,] SKIP IF CHAR IN INPUT
                case TTCALLs.SKPINL: // [TTCALL 14,] SKIP IF LINE IN INPUT
                case TTCALLs.IONEOU: // [TTCALL 15,] OUTPUT IMAGE CHAR

                default:
                    throw new InstructionFailure(processor,
                                                 InstructionExit.UnimplimentedUUO,
                                                 processor.PC, instruction,
                                                 opcode,
                                                 ac,
                                                 ea,
                                                 "Unimplemented TTCALL: " + (TTCALLs) ac,
                                                 null);
            }

            return InstructionExit.Normal;
        }

        private void OutputASCIZ(SimhPDP10CPU processor, ulong ea)
        {
            var sb = new StringBuilder();
            var eos = false;

            while (!eos)
            {
                var output = ASCIZ.Get(processor.CORE[ea]);
                foreach (var c in output)
                {
                    if (c == 0)
                    {
                        eos = true;
                        break;
                    }
                    sb.Append(c);
                }
                ea++;
            }

            if (ConsoleOutput != null)
                ConsoleOutput(this, new ConsoleOutputEvent(sb.ToString()));
        }

        public void OUTSTR(string outline)
        {
            if (ConsoleOutput != null)
                ConsoleOutput(this, new ConsoleOutputEvent(outline));
        }

        public void OUTSTRline(string outline)
        {
            OUTSTR(outline + "\r\n");
        }
    }
}