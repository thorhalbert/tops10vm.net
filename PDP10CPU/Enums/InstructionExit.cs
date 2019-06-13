namespace PDP10CPU.Enums
{
    public enum InstructionExit
    {
        Normal,
        XCT,
        MUUO,
        SingleStep,
        BreakPoint,
        Halt,
        UnimplimentedUUO,
        RegisterIndirectionLimitExceeded,
        IllegalInstructionZero,
        XCTMaxDepthExceeded,
        UnimplementedMonitorCall,
        IntentionalExit,
    }
}