using System;
using ThirtySixBits;

namespace PDP10CPU.Events
{
    public class LightsChangedEvent : EventArgs
    {
        public Word36 OldLights { get; private set; }
        public Word36 NewLights { get; private set; }

        public LightsChangedEvent(Word36 oldl, Word36 newl)
        {
            OldLights = oldl;
            NewLights = newl;
        }
    }
}