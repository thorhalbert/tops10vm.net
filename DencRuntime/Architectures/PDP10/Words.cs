using System;
using ThirtySixBits;

namespace Thorsbrain.Denc.Runtime.Architectures.PDP10
{
    public class Words
    {
        private Accessor accessor;

        public Words(PDP10Class parent, int startAddress, int length)
        {
            accessor = parent.RootAccessor;
            Length = length;
        }

        public int Length { get; private set; }

        public Word36 this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}