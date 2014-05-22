using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using NLog;

namespace CIt
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		MainWindowController controller;

		public AppDelegate()
		{
		}

		public override void FinishedLaunching(NSObject notification)
		{
			if (controller == null)
			{
				controller = new MainWindowController();
				controller.Window.MakeKeyAndOrderFront(this);
			}
		}

		public override bool OpenFile(NSApplication sender, string filename)
		{
			logger.Info("OpenFile '{0}'", filename);

			if (controller == null)
			{
				controller = new MainWindowController();
				controller.Window.MakeKeyAndOrderFront(this);
			}

			return controller.OpenFolderOrFile(filename);
		}
	}
}
