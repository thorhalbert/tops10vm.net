using System;
using System.Collections;
using System.Collections.Generic;
using PDP10CPU.Events;
using ThirtySixBits;

namespace PDP10CPU.Memory
{
    public class Page : IEnumerable<Word36>
    {
        public event EventHandler<PageChangedEvent> PageChanged;

        public MemoryProtection Protection { get; private set; }
        public int PageSize { get; private set; }

        public bool PageDirty { get; set; }

        private readonly Word36[] pageMem;

        public Word36 this[int i]
        {
            get { return pageMem[i]; }
            set
            {
                if (!pageMem[i].Equals(value))
                {
                    PageDirty = true;
                    if (PageChanged != null)
                        PageChanged(this, new PageChangedEvent(i, pageMem[i], value));
                    pageMem[i] = value;
                }
            }
        }

        public Word36 this[Word18 i]
        {
            get { return this[(int) i.UI]; }
            set { this[(int) i.UI] = value; }
        }

        public Word36 this[Word36 i]
        {
            get { return this[(int) i.UL]; }
            set { this[(int) i.UL] = value; }
        }

        public Page() : this(512) {}

        public Page(int pageSize)
        {
            PageSize = pageSize;

            pageMem = new Word36[pageSize];
            Protection = new MemoryProtection {Writable = true};
        }

        #region Implementation of IEnumerable

        public IEnumerator<Word36> GetEnumerator()
        {
            return (IEnumerator<Word36>) pageMem.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}