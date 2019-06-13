using System;
using PDP10CPU.Memory;

namespace PDP10CPU.Events
{
    public class PageFaultError : Exception
    {
        public UserModeCore CORE { get; private set; }
        public int Segment { get; private set; }
        public int Page { get; private set; }
        public ulong Address { get; private set; }

        public PageFaultError(UserModeCore core, int segment, int page, ulong loc)
        {
            CORE = core;
            Segment = segment;
            Page = page;
            Address = loc;
        }
    }
}