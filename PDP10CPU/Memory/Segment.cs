using System;
using System.Collections;
using System.Collections.Generic;
using PDP10CPU.Events;

namespace PDP10CPU.Memory
{
    public class Segment : IEnumerable<Page>
    {
        public bool EnablePageChangeEvents { get; set; }

        public event EventHandler<SegmentChangedEvent> SegmentChanged;

        public event EventHandler<MemberPageChangedEvent> MemberPageChanged;

        public readonly Page[] SegmentMem;
        // This currently limits a page from being mapped to more than one spot
        // I suspect this is ok for Tops-10
        public readonly Dictionary<Page, int> pageLookup = new Dictionary<Page, int>();

        private readonly int pageSize;

        public Segment() : this(512, false) {}

        public Segment(int pageSize, bool trackChangeEvents)
        {
            var maxPagesPerSegment = UserModeCore.MaxSegmentSize/pageSize;
            SegmentMem = new Page[maxPagesPerSegment];
            this.pageSize = pageSize;

            EnablePageChangeEvents = trackChangeEvents;
        }

        public Page this[int x]
        {
            get { return SegmentMem[x]; }
            set
            {
                if (SegmentChanged != null)
                    SegmentChanged(this, new SegmentChangedEvent(x, SegmentMem[x], value));
                if (SegmentMem[x] != null)
                    destroyPage(SegmentMem[x], x);
                SegmentMem[x] = value;
            }
        }

        public void NewPage(int Page)
        {
            var page = new Page(pageSize) {Protection = {Writable = true}};
            this[Page] = page;
            pageLookup.Add(page, Page);

            if (EnablePageChangeEvents)
                page.PageChanged += pagePageChanged;
        }

        public void KillPage(int x)
        {

            var page = this[x];

            if (EnablePageChangeEvents)
                page.PageChanged -= pagePageChanged;

            this[x] = null;


        }

        private void pagePageChanged(object sender, PageChangedEvent e)
        {
            if (!EnablePageChangeEvents) return;

            var page = (Page) sender;
            var pagenum = pageLookup[page];

            if (MemberPageChanged != null)
                MemberPageChanged(this, new MemberPageChangedEvent(page, pagenum, e));
        }

        private void destroyPage(Page page, int x)
        {
            if (pageLookup.ContainsKey(page))
                pageLookup.Remove(page);

            try
            {
                page.PageChanged -= pagePageChanged;
            }
            catch {}
        }

        public int WhichPage(Page inPage)
        {
            return pageLookup[inPage];
        }

        public void SegmentDestroy(int x)
        {
            if (SegmentChanged!=null)
                SegmentChanged(this, new SegmentChangedEvent(x,null,null));
        }

        #region Implementation of IEnumerable

        public IEnumerator<Page> GetEnumerator()
        {
            IEnumerable<Page> enumeration = SegmentMem;
            return enumeration.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

      
    }
}