using PDP10CPU;
using PDP10CPU.CPU;
using PDP10CPU.Enums;

namespace Monitor.Subsystems.UUOHandlers
{
    public interface IUUOHandler
    {
        void Setup(MonitorContext ctx);

        InstructionExit HandleUUO(SimhPDP10CPU processor, ulong instruction,
                                  OpCodes opcode, int ac, ulong ea);
    }
}