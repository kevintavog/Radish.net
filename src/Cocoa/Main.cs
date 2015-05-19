using MonoMac.AppKit;
using System.Threading;

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

