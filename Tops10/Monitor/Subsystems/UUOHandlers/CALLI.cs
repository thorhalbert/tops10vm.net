using Monitor.Subsystems.UUOs.CALLIs;
using PDP10CPU;
using PDP10CPU.CPU;
using PDP10CPU.Enums;
using Symbols;

namespace Monitor.Subsystems.UUOHandlers
{
    public class CALLI : IUUOHandler
    {
        private MonitorContext Monitor;

        public void Setup(MonitorContext ctx)
        {
            Monitor = ctx;
        }

        public InstructionExit HandleUUO(SimhPDP10CPU processor, ulong instruction, OpCodes opcode, int ac, ulong ea)
        {
            switch ((CALLIs) ea)
            {
                case CALLIs.GETTAB:
                    return GETTAB.Execute(Monitor, processor, instruction, opcode, ac, ea);
                case CALLIs.CORE:
                    return CORE.Execute(Monitor, processor, instruction, opcode, ac, ea);
                case CALLIs.EXIT:
                    return InstructionExit.IntentionalExit;
                default:
                    return InstructionExit.UnimplementedMonitorCall;
            }
        }
    }
}