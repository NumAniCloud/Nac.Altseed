﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nac.Altseed.Test.Selector;
using Nac.Altseed.Test.Tests;
using asd;

namespace Nac.Altseed.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			var test = new Input.JoyStickTest();
			test.Run();
		}
	}
}
