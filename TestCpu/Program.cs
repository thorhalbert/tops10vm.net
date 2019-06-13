using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileFormats;
using PDP10CPU.Memory;

namespace TestCpu
{
	class Program
	{
		static void Main(string[] args)
		{
			var Core = new UserModeCore();

		    var loader = new Tops10SAVLoader(Core,"test.sav");


		}
	}
}
