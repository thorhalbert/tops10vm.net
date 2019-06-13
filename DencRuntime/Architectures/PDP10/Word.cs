using System;
using ThirtySixBits;

namespace Thorsbrain.Denc.Runtime.Architectures.PDP10
{
    public class Word
    {
        private Accessor accessor;

        public Word(PDP10Class parent, int startAddress)
        {
            accessor = parent.RootAccessor;
        }

        public Word36 GetValue
        {
            get { throw new NotImplementedException(); }
        }

        public void SetValue(Word36 value)
        {
            throw new NotImplementedException();
        }
    }
}