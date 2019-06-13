using System;
using PDP10CPU.CPU;
using PDP10CPU.Enums;
using ThirtySixBits;

namespace PDP10CPU.Exceptions
{
    public class InstructionFailure : Exception
    {
        public SimhPDP10CPU CPU { get; private set; }
        public Word36 Instruction { get; private set; }
        public OpCodes Opcode { get; private set; }
        public ulong PC { get; private set; }
        public int AC { get; private set; }
        public ulong EA { get; private set; }
        public InstructionExit ExitType { get; private set; }

        public InstructionFailure(
            SimhPDP10CPU cpu,
            InstructionExit exitType,
            ulong pc,
            ulong inst,
            OpCodes opcode,
            int ac,
            ulong ea,
            string message,
            Exception inner) : base(message, inner)
        {
            CPU = cpu;
            Instruction = new Word36(inst);
            PC = pc;
            AC = ac;
            EA = ea;
            Opcode = opcode;
            ExitType = exitType;
        }
    }
}