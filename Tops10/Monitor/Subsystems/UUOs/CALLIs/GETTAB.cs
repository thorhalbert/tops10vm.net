using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDP10CPU;
using PDP10CPU.CPU;
using PDP10CPU.Enums;

namespace Monitor.Subsystems.UUOs.CALLIs
{
    internal static class GETTAB
    {
        public static InstructionExit Execute(MonitorContext monitor, SimhPDP10CPU processor, ulong instruction, OpCodes opcode, int ac, ulong ea)
        {
            throw new NotImplementedException();
        }
    }
}
