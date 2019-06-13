using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CommandProcessor;
using Monitor;
using Monitor.Subsystems.Console;
using PDP10CPU.CPU;
using PDP10CPU.Enums;
using PDP10CPU.Events;
using PDP10CPU.Exceptions;
using PDP10CPU.Memory;
using Symbols;
using ThirtySixBits;

namespace UnitTestCPU
{
    public class MarshallCPU
    {
        private readonly UserModeCore core;
        private readonly SimhPDP10CPU cpu;
        private readonly MonitorContext tops10;
        private readonly SimpleParser parser;

        public bool DiaMon { get; set; }

        private const string CRLF = "\r\n";

        private ulong CycleStart;

        private readonly TextWriter outputChan;
        private readonly TextReader inputChan;

        public string ResultHash { get; private set; }

        private readonly StringBuilder outputContent = new StringBuilder();

        public MarshallCPU(TextWriter outputChan, TextReader inputChan)
        {
            this.outputChan = outputChan;
            this.inputChan = inputChan;

            if (this.outputChan != null)
                this.outputChan.NewLine = CRLF;

            core = new UserModeCore(false);

            cpu = new SimhPDP10CPU(core, OSTypes.Tops10)
                      {
                          ProcessorType = ProcessorTypes.KL10
                      };

            tops10 = new MonitorContext(cpu);

            tops10.TTCALL.ConsoleOutput += TTCALL_ConsoleOutput;
            tops10.TTCALL.AttachToConsole();

            cpu.ProcFlags = 0;
            cpu.SetUserMode();

            parser = new SimpleParser {CPU = cpu};
        }

        public void ProcessCommandFile()
        {
            var run = true;

            while (run)
            {
                var inp = inputChan.ReadLine();
                if (inp == null) break;
                if (inp.Trim() == "EOF") break;

                try
                {
                    var action = parser.Parse(inp);

                    switch (action)
                    {
                        case ParserActions.Ok:
                            break; // All good, no action
                        case ParserActions.Unimplemented:
                            WriteLine("?Unimplemented Command: " + inp);
                            run = false;
                            break;
                        case ParserActions.BadCommand:
                            WriteLine("?Bad or Unknown Command: " + inp);
                            run = false;
                            break;
                        case ParserActions.Start:
                            run = runCPU();
                            break;
                    }
                }
                catch (InstructionFailure fail)
                {
                    WriteLine("?Instruction Failure: " + fail.Message);
                    WriteLine("[PC:" + fail.PC.ToOctal(8) + "/" + fail.Instruction + "<Opcode " + fail.Opcode + "> EA:" +
                              fail.EA.ToOctal(6) + "]");
                }
                catch (PageFaultError fault)
                {
                    WriteLine("?Page Fault: Location:" + fault.Address.ToOctal(6) + " Seg:" +
                              fault.Segment.ToOctal(1) + ":" + fault.Page.ToOctal(3));
                }
                catch (Exception ex)
                {
                    WriteLine("?Uncaught error: " + ex.Message);
                    run = false;
                }
            }

            WriteLine("[EOF]");
            procEOF();
        }

        private bool runCPU()
        {
            CycleStart = cpu.InstructionsExecuted;
            cpu.Runmode = RunModes.FreeRun;

            // Start at .JBSA
            cpu.PC = cpu.CORE[(int) JOBDAT._JBSA].UL;
            if (cpu.PC < 140.OctU())
                cpu.PC = 30001.OctU(); // Assumption for diagnostics

            if (DiaMon)
                cpu.PC = 30010.OctU();

            var procStat = cpu.ProcessorMainloop();

            var cyclesRun = cpu.InstructionsExecuted - CycleStart;

            switch (procStat)
            {
                case InstructionExit.BreakPoint:
                    tops10.TTCALL.OUTSTRline("[Breakpoint Reached at PC:" + cpu.PC.ToOctal(6) +
                                             " <Cycles " + cyclesRun + ">" + "]");
                    return true;
                    //case InstructionExit.SingleStep:
                    //    TOPS10.TTCALL.OUTSTRline("[SingleStep]");
                    //    return true;
                case InstructionExit.IntentionalExit:
                    tops10.TTCALL.OUTSTRline("[Exit " + " <Cycles " + cyclesRun + ">]");
                    return true;
                default:
                    tops10.TTCALL.OUTSTRline("?Program Exited: " + procStat + " at PC:" + cpu.PC.ToOctal(6) +
                                             " <Cycles " + cyclesRun + ">");
                    return false;
            }
        }

        private void procEOF()
        {
            var outp = outputContent.ToString();
            var ba = Encoding.ASCII.GetBytes(outp);

            var sha1 = new SHA1Managed().ComputeHash(ba);

            var sb = new StringBuilder();
            foreach (var b in sha1)
                sb.Append(b.ToString("X2"));

            ResultHash = sb.ToString();

            WriteLine("Hash: " + ResultHash);

            outputChan.Close();
        }

        private void TTCALL_ConsoleOutput(object sender, ConsoleOutputEvent e)
        {
            WriteConsole(e.Output);
        }

        private void WriteConsole(string output)
        {
            if (outputChan != null)
                outputChan.Write(output);

            Console.Write(output);

            outputContent.Append(output);
        }

        private void WriteLine(string output)
        {
            WriteConsole(output + CRLF);
        }
    }
}