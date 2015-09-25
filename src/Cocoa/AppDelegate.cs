using System;
using NLog;
using Radish.Support;
using System.IO;
using Rangic.Utilities.Preferences;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Radish
{
    public partial class AppDelegate : NSApplicationDelegate
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private MainWindowController controller;
        private string filename;

        public override void DidFinishLaunching(NSNotification notification)
        {
            Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");
            var urlList = new NSFileManager().GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User);
            Preferences<RadishPreferences>.Load(Path.Combine(
                urlList[0].Path,
                "Preferences",
                "Radish.rangic.json"));
            
            Rangic.Utilities.Geo.OpenStreetMapLookupProvider.UrlBaseAddress = Preferences<RadishPreferences>.Instance.BaseLocationLookup;
            logger.Info("Resolving placenames via {0}", Rangic.Utilities.Geo.OpenStreetMapLookupProvider.UrlBaseAddress);

			controller = new MainWindowController();
			controller.Window.MakeKeyAndOrderFront(this);

            if (filename != null)
            {
                controller.OpenFolderOrFile(filename);
            }
		}

		public override bool OpenFile(NSApplication sender, string filename)
		{
            Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");
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
