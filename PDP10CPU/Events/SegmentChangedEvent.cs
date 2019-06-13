using System;
using PDP10CPU.Memory;

namespace PDP10CPU.Events
{
    public class SegmentChangedEvent : EventArgs
    {
        public int PageNum { get; private set; }
        public Page OldPage { get; private set; }
        public Page NewPage { get; private set; }

        public SegmentChangedEvent(int x, Page page, Page value)
        {
            PageNum = x;
            OldPage = page;
            NewPage = value;
        }
    }
}