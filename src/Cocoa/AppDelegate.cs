using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using NLog;
using Radish.Support;
using System.IO;

namespace Radish
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
            var urlList = new NSFileManager().GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User);
            Preferences.Load(Path.Combine(
                urlList[0].Path,
                "Preferences",
                "com.rangic.Radish.json"));

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
