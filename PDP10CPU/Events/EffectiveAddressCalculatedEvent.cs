using System;

namespace PDP10CPU.Events
{
    public class EffectiveAddressCalculatedEvent : EventArgs
    {
        public ulong EA { get; private set; }

        public EffectiveAddressCalculatedEvent(ulong ea)
        {
            EA = ea;
        }
    }
}