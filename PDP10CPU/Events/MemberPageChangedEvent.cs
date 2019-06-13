using System;
using PDP10CPU.Memory;

namespace PDP10CPU.Events
{
    public class MemberPageChangedEvent : EventArgs
    {
        public Page Page { get; private set; }
        public int PageNum { get; private set; }
        public PageChangedEvent PageChange { get; private set; }

        public MemberPageChangedEvent(Page page, int pagenum, PageChangedEvent e)
        {
            Page = page;
            PageNum = pagenum;
            PageChange = e;
        }
    }
}