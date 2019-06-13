using System;
using System.Collections;
using System.Collections.Generic;
using PDP10CPU.Events;
using ThirtySixBits;

namespace PDP10CPU.Memory
{
    public class UserModeCore : IEnumerable<Segment>
    {
        private readonly int pageSize = 512;
        private readonly int maxPagesPerSegment;
        public const int MaxSegmentSize = 0x40000;

        public bool EnablePageChangeEvents { get; set; }

        private readonly int initialSegments = 4; // For testing

        private Segment[] coreMemory;

        public Word36 this[long loc]
        {
            get { return this[CurrentSegment, (ulong) loc]; }
            set { this[CurrentSegment, (ulong) loc] = value; }
        }

        public Word36 this[ulong loc]
        {
            get { return this[CurrentSegment, loc]; }
            set { this[CurrentSegment, loc] = value; }
        }

        //public Word36 this[Word18 loc]
        //{
        //    get { return this[CurrentSegment, loc.UI]; }
        //    set { this[CurrentSegment, loc.UI] = value; }
        //}

        public Word36 this[int segment, long loc]
        {
            get { return this[segment, (ulong) loc]; }
            set { this[segment, (ulong) loc] = value; }
        }

        //public Word36 this[int segment, Word18 loc]
        //{
        //    get { return this[segment,loc.UI]; }
        //    set { this[segment, loc.UI] = value; }
        //}

        public Word36 this[int segment, ulong loc]
        {
            get
            {
                var mem = (int) loc%pageSize; // Should use ands and shifts here for speed
                var page = (int) loc/pageSize;
                if (coreMemory[segment][page] == null)
                    throw new PageFaultError(this, segment, page, loc);
                return coreMemory[segment][page][mem];
            }
            set
            {
                var mem = (int) loc%pageSize; // Should use ands and shifts here for speed
                var page = (int) loc/pageSize;
                coreMemory[segment][page][mem] = value;
            }
        }

        public int CurrentSegment { get; set; }

        public int PageSize
        {
            get { return pageSize; }
        }

        public int MaxPagesPerSegment
        {
            get { return maxPagesPerSegment; }
        }

        public int InitialSegments
        {
            get { return initialSegments; }
        }

        public UserModeCore() : this(512, 1, 1, false)
        {
        }

        public UserModeCore(bool trackChangeEvents) : this(512, 1, 1, trackChangeEvents)
        {
        }

        public UserModeCore(int initialPages, int setMaxSegments, bool trackChangeEvents)
            : this(512, initialPages, setMaxSegments, trackChangeEvents)
        {
        }

        public UserModeCore(int setPageSize, int initialPages, int InitialSegments, bool trackChangeEvents)
        {
            EnablePageChangeEvents = trackChangeEvents;

            if (setPageSize > 0)
                pageSize = setPageSize;

            CurrentSegment = 0; // Start here

            if ((MaxSegmentSize%pageSize) != 0)
                throw new Exception("Pagesize must be a power of 2");

            maxPagesPerSegment = MaxSegmentSize/pageSize;

            initialSegments = 1;
            if (InitialSegments > 0)
                initialSegments = InitialSegments;

            Clear();
        }

        /// <summary>
        ///   Erase core and start over - this needs to fire an event probably
        /// </summary>
        public void Clear()
        {
            coreMemory = new Segment[initialSegments];

            for (var seg = 0; seg < InitialSegments; seg++)
            {
                coreMemory[seg] = new Segment(pageSize, EnablePageChangeEvents);

                initializeSegment(seg, 1);
            }
        }

        private void initializeSegment(int seg, int initialPages)
        {
            for (var p = 0; p < initialPages; p++)
                NewPage(seg, p);
        }

        public void NewPage(int segment, int page)
        {
            coreMemory[segment].NewPage(page);
        }

        public void NewPage(int Page)
        {
            NewPage(0, Page);
        }

        public void KillPage(int segment, int page)
        {
            coreMemory[segment].KillPage(page);
        }

        public void KillPage(int page)
        {
            coreMemory[0].KillPage(page);
        }

        public bool PageExists(int segment, int page)
        {
            return coreMemory[segment][page] != null;
        }

        public bool PageExists(int Page)
        {
            return PageExists(0, Page);
        }

        public MemoryProtection PageProtection(int segment, int page)
        {
            return coreMemory[segment][page].Protection;
        }

        public MemoryProtection PageProtection(int page)
        {
            return PageProtection(CurrentSegment, page);
        }

        public int Page(long mem)
        {
            return (int) mem/pageSize;
        }

        public void Tops10Core(Word18 hiseg, Word18 loseg)
        {
            const int segments = 1; // Only one seg for tops10

            var hiPage = 0U;
            var loPage = 0U;

            if (hiseg.NZ)
                hiPage = (hiseg.UI + 1)/(uint) pageSize;

            if (loseg.NZ)
                loPage = (loseg.UI + 1)/(uint) pageSize;

            var hiStart = 400000.Oct()/pageSize;

            // Make sure we have a segment
            if (coreMemory[0] == null)
            {
                coreMemory[0] = new Segment(pageSize, EnablePageChangeEvents);
                initializeSegment(0, 1);
            }

            // Get rid of spurious segments

            for (var seg = 1; seg < coreMemory.Length; seg++)
            {
                if (seg <= segments) continue;

                if (coreMemory[seg] == null) continue;

                coreMemory[seg].SegmentDestroy(seg);
                coreMemory[seg] = null;
            }

            if (loseg.NZ)
                preenPages(hiStart, MaxPagesPerSegment, loPage);

            if (hiseg.NZ)
                preenPages(0, hiStart - 1, hiPage);
        }

        private void preenPages(int start, int hiStart, uint loPage)
        {
            for (var loCount = start; loCount < hiStart; loCount++)
                if (loCount <= loPage)
                {
                    if (!PageExists(loCount))
                        NewPage(loCount);
                }
                else
                    KillPage(loCount);
        }

        #region Implementation of IEnumerable

        public IEnumerator<Segment> GetEnumerator()
        {
            IEnumerable<Segment> enumeration = coreMemory;

            return enumeration.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}