using System;
using PDP10CPU;
using PDP10CPU.CPU;
using PDP10CPU.Enums;
using ThirtySixBits;

namespace Monitor.Subsystems.UUOs.CALLIs
{
    internal static class CORE
    {
        public static InstructionExit Execute(MonitorContext monitor, SimhPDP10CPU processor, ulong instruction,
                                              OpCodes opcode, int ac, ulong ea)
        {

            var pageSize = processor.CORE.PageSize;

            // Ignore PHY for now - no physical mode anyway (we may want an emulator assert here though)

            var acV = processor.CORE[ac];
            var hiseg = acV.LHW;
            var loseg = acV.RHW;

            // Error states

            // High and low are zero
            if (hiseg.Z && loseg.Z)
                return InstructionExit.Normal;

            // Hiseg specified but not in high seg (clears - high segment, but can only be done from loseg)
            if (processor.PC >= 400000.OctU() &&
                hiseg.NZ &&
                hiseg < 400001.Oct18())
                return InstructionExit.Normal;

            // Lowseg is running over the hiseg
            if (loseg.NZ &&
                loseg > 399999.Oct18())
                return InstructionExit.Normal;

           

            processor.CORE.Tops10Core(hiseg,loseg);

            processor.PC++; // Success Exit
            return InstructionExit.Normal;
        }
    }
}