using System;

namespace PDP10CPU.Events
{
    public class PCChangedEvent : EventArgs
    {
        public ulong OldPC { get; private set; }
        public ulong NewPC { get; private set; }

        public PCChangedEvent(ulong oldPC, ulong newPC)
        {
            OldPC = oldPC;
            NewPC = newPC;
        }
    }
}