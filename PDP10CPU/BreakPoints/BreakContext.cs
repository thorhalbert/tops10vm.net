using PDP10CPU.Memory;

namespace PDP10CPU.BreakPoints
{
    public class BreakContext
    {
        public BreakPointActions BreakAction { get; set; }
        public int ValueAddress { get; set; }
        public BreakCompareTypes CompareType { get; set; }
        public BreakWords WordType { get; set; }
        public ulong CompareValue { get; set; }

        public bool DoIPause(UserModeCore core, ulong PC)
        {
            switch (BreakAction)
            {
                case BreakPointActions.Pause:
                    return true;
                case BreakPointActions.PauseOnValue:
                    return pauseOnValue(core, PC);
            }
            return false; // ?
        }

        private bool pauseOnValue(UserModeCore core, ulong pc)
        {
            var uMem = core[ValueAddress];

            ulong v = 0;
            switch (WordType)
            {
                case BreakWords.Whole:
                    v = uMem.UL;
                    break;
                case BreakWords.LH:
                    v = uMem.LHW.UL;
                    break;
                case BreakWords.RH:
                    v = uMem.RHW.UL;
                    break;
            }

            switch (CompareType)
            {
                case BreakCompareTypes.EQ:
                    return v == CompareValue;
                case BreakCompareTypes.LT:
                    return v < CompareValue;
                case BreakCompareTypes.LE:
                    return v <= CompareValue;
                case BreakCompareTypes.GT:
                    return v > CompareValue;
                case BreakCompareTypes.GE:
                    return v >= CompareValue;
            }

            return false; // ?
        }
    }
}