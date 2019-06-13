using ThirtySixBits;
using Thorsbrain.Denc.Runtime;
using Thorsbrain.Denc.Runtime.Architectures;
using Thorsbrain.Denc.Runtime.Architectures.PDP10;

namespace test1.test2.test3
{
	public partial class IOTESTp : PDP10Class
	{
		private Word __FIRST;
		public Word36 _FIRST{ get { return __FIRST.GetValue; } set { __FIRST.SetValue(value); } }
		private Words __SECOND;
		public Words _SECOND{ get { return __SECOND; } }
		private Words __THIRD;
		public Words _THIRD{ get { return __THIRD; } }
		public IOTESTp()
		{
			__FIRST = new Word(this, 0);
			__SECOND = new Words(this, 1, 3);
			__THIRD = new Words(this, 4, 4);
		}
	}
	public partial class FREDdBILL : PDP10Class
	{
		private Words _element0;
		public partial class SUBTEST2 : PDP10Class
		{
			private Words _NAME;
			public Words NAME{ get { return _NAME; } }
			public SUBTEST2()
			{
				_NAME = new Words(this, 0, 4);
			}
		}
		private Words _element4;
		public FREDdBILL()
		{
			_element0 = new Words(this, 0, 3);
			_element4 = new Words(this, 4, 2);
		}
	}
}
