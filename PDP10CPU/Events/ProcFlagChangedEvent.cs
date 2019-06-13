using System;

namespace PDP10CPU.Events
{
    public class ProcFlagChangedEvent : EventArgs
    {
        public ulong OldFlag { get; private set; }
        public ulong NewFlag { get; private set; }

        public string SerializedFlags { get; private set; }

        public ProcFlagChangedEvent(ulong oldFlag, ulong newFlag, string serializedValues)
        {
            OldFlag = oldFlag;
            NewFlag = newFlag;

            SerializedFlags = serializedValues;
        }
    }
}