using System;
using System.Drawing;
using System.Threading;
using AppKit;

namespace Radish
{
	class MainClass
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.Name = "Main";
			NSApplication.Init();
			NSApplication.Main(args);
		}
	}
}

