using System;
using ThirtySixBits;

namespace PDP10CPU.Events
{
    public class PageChangedEvent : EventArgs
    {
        public int Index { get; private set; }
        public Word36 OldValue { get; private set; }
        public Word36 NewValue { get; private set; }

        public PageChangedEvent(int index, Word36 oldValue, Word36 newValue)
        {
            Index = index;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}