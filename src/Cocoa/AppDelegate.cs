using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using NLog;
using Radish.Support;
using System.IO;
using Rangic.Utilities.Preferences;

namespace Radish
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private MainWindowController controller;
        private string filename;

		public AppDelegate()
		{
		}

		public override void FinishedLaunching(NSObject notification)
		{
            var urlList = new NSFileManager().GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User);
            Preferences<RadishPreferences>.Load(Path.Combine(
                urlList[0].Path,
                "Preferences",
                "Radish.rangic.json"));

			controller = new MainWindowController();
			controller.Window.MakeKeyAndOrderFront(this);

            if (filename != null)
            {
                controller.OpenFolderOrFile(filename);
            }
		}

		public override bool OpenFile(NSApplication sender, string filename)
		{
            if (controller == null)
            {
                this.filename = filename;
    			logger.Info("OpenFile '{0}'", filename);
                return true;
            }

            return controller.OpenFolderOrFile(filename);
		}
	}
}
