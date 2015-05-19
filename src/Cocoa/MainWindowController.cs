using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ImageIO;
using NLog;
using Radish.Controllers;
using Radish.Models;
using System.Threading.Tasks;
using System.Threading;

namespace Radish
{
	public partial class MainWindowController : NSWindowController, IFileViewer
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private const string TrashSoundPath = @"/System/Library/Components/CoreAudio.component/Contents/SharedSupport/SystemSounds/dock/drag to trash.aif";
		static private HashSet<string> SupportedFileTypes = new HashSet<string>(CGImageSource.TypeIdentifiers);


		private MediaListController		mediaListController;
		private string					currentlyDisplayedFile;
		private System.Timers.Timer     hideNotificationTimer = new System.Timers.Timer(250);
        private ZoomView                zoomView;



#region Constructors

		// Called when created from unmanaged code
		public MainWindowController(IntPtr handle) : base(handle)
		{
			Initialize();
		}
		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base(coder)
		{
			Initialize();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController() : base("MainWindow")
		{
			Initialize();
		}
		// Shared initialization code
		void Initialize()
		{
			mediaListController = new DirectoryController(this, FileListUpdated);
			SupportedFileTypes.Add("com.adobe.pdf");
		}

#endregion


        public new MainWindow Window { get { return (MainWindow)base.Window; } }
		public NSWindow NotificationWindow { get { return (NSWindow)notificationWindow; } }
        public NSWindow BusyWindow { get { return (NSWindow)busyWindow; } }
		public FileInformationController InformationController { get { return (FileInformationController)fileInformationController; } }
        public SearchController SearchController { get { return (SearchController)searchController; } }
        public ThumbController ThumbController { get { return (ThumbController)thumbController; } }

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

            zoomView = new ZoomView(imageView);
			Window.BackgroundColor = NSColor.DarkGray;

            busyProgressControl.StartAnimation(this);
            ((KTRoundedView) busyWindow.ContentView).BackgroundColor = NSColor.Yellow;


			hideNotificationTimer.Elapsed += (s, e) =>
			{
				InvokeOnMainThread( () => HideNotification() );
			};

            statusGps.StringValue = "";
            statusIndex.StringValue = "";
            statusKeywords.StringValue = "";
            statusTimestamp.StringValue = "";
            statusFilename.StringValue = "";

            ThumbController.SetMediaListController(mediaListController);
		}

        private void ShowFile(bool forceRefresh = false)
		{
            NSCursor.SetHiddenUntilMouseMoves(true);
			if (mediaListController.Count < 1)
			{
				currentlyDisplayedFile = null;
                imageView.Image = null;
				Window.Title = "<No files>";
				UpdateStatusBar();
				return;
			}

            if (forceRefresh)
            {
                currentlyDisplayedFile = null;
            }

			var mm = mediaListController.Current;
			if (mm.FullPath != currentlyDisplayedFile)
			{
				logger.Info("ShowFile: {0}; {1}", mediaListController.CurrentIndex, mm.FullPath);
				currentlyDisplayedFile = mm.FullPath;
                var index = mediaListController.CurrentIndex;

                Task.Run( () =>
                {
                    try
                    {
                        NSUrl url;
                        if (File.Exists(mm.FullPath))
                            url = NSUrl.FromFilename(mm.FullPath);
                        else
                            url = NSUrl.FromString(mm.FullPath);

                        using (var imageSource = CGImageSource.FromUrl(url))
                        {
                            using (var cgi = imageSource.CreateImage(0, null))
                            {
                                base.InvokeOnMainThread( () => 
                                {
                                    using (var image = new NSImage(cgi, new SizeF(cgi.Width, cgi.Height)))
                                    {
                                        imageView.Image = image;
                                    }
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Exception loading & displaying image: " + e);
                    }
                });
            }

            Window.Title = mm.Name;
			UpdateStatusBar();

			if (informationPanel.IsVisible)
			{
				InformationController.SetFile(mm);
			}
		}

		private void UpdateStatusBar()
		{
			if (mediaListController.Count < 1)
			{
                statusFilename.StringValue = statusTimestamp.StringValue = statusGps.StringValue = statusKeywords.StringValue = "";
				statusIndex.StringValue = "No files";
				return;
			}

			var mm = mediaListController.Current;

            statusFilename.StringValue = mm.Name;

			var timestamp = mm.Timestamp.ToString("yyyy/MM/dd HH:mm:ss");
			if (mm.FileAndExifTimestampMatch)
			{
				statusTimestamp.StringValue = timestamp;
			}
			else
			{
				var str = new NSMutableAttributedString(timestamp);
				str.AddAttribute(
					NSAttributedString.ForegroundColorAttributeName, 
					NSColor.Yellow,
					new NSRange(0, timestamp.Length));
				str.AddAttribute(
					NSAttributedString.UnderlineStyleAttributeName, 
					new NSNumber((int) NSUnderlineStyle.Single), 
					new NSRange(0, timestamp.Length));
				statusTimestamp.AttributedStringValue = str;
			}
			statusIndex.StringValue = String.Format(
				"{0} of {1}", 
				mediaListController.CurrentIndex + 1, 
				mediaListController.Count);

            statusKeywords.StringValue = mm.Keywords;
            statusGps.StringValue = "";

            if (mm.HasPlaceName)
            {
                SetStatusGps(mm);
            }
            else
            {
                ClearStatusGps();
                Task.Run( () =>
                {
                    mm.ToPlaceName();
                    BeginInvokeOnMainThread( () => 
                    {
                        SetStatusGps(mm);
                    });
                });
            }
		}

        private void ClearStatusGps()
        {
            statusGps.StringValue = "";
        }

        private void SetStatusGps(MediaMetadata mm)
        {
            if (mm.HasPlaceName && !String.IsNullOrEmpty(mm.ToPlaceName()))
            {
                statusGps.StringValue = mm.ToPlaceName();
            }
            else
            {
                statusGps.StringValue = mm.ToDms();
            }
        }

        public bool OpenFolderOrFile(string path)
        {
            return OpenFolderOrFiles(new string[] { path } );
        }

		public bool OpenFolderOrFiles(string[] paths)
		{
			string filename = null;
			if (File.Exists(paths[0]))
			{
				filename = paths[0];

                if (paths.Length == 1)
                {
                    paths[0] = Path.GetDirectoryName(filename);
                }
			}

            ShowBusy();
            var dirController = mediaListController as DirectoryController;
            if (dirController == null)
            {
                dirController = new DirectoryController(this, FileListUpdated);
                mediaListController = dirController as MediaListController;
            }

            logger.Info("Open '{0}'", String.Join("', '", paths));
			dirController.Scan(paths);
			mediaListController.SelectFile(filename);
			ShowFile();

            // HACK: I don't understand the white band that shows below the image view and above the status view.
            // Causing the window to resize or hiding/showing the view forces it to redo whatever is needed. ???
            imageView.Hidden = true;
            imageView.Hidden = false;

            ThumbController.SetMediaListController(mediaListController);

			// That's gross - Mono exposes SharedDocumentController as NSObject rather than NSDocumentcontroller
			(NSDocumentController.SharedDocumentController as NSDocumentController).NoteNewRecentDocumentURL(new NSUrl(paths[0], false));

            HideBusy();

			return true;
		}

		private void FileListUpdated()
		{
			mediaListController.SelectFile(currentlyDisplayedFile);
			ShowFile();
		}

		private void ShowNotification(NotificationGraphic graphic)
		{
			notificationImage.Image = NSImage.ImageNamed(graphic.ToString());

			var mainFrame = Window.Frame;
            var origin = new PointF(
				mainFrame.X + ((mainFrame.Width - NotificationWindow.Frame.Width) / 2),
				mainFrame.Y + ((mainFrame.Height - NotificationWindow.Frame.Height) / 2));
			NotificationWindow.SetFrameOrigin(origin);
			NotificationWindow.MakeKeyAndOrderFront(this);

			hideNotificationTimer.Stop();
			hideNotificationTimer.Start();
		}

		private void HideNotification()
		{
			NotificationWindow.OrderOut(this);
			hideNotificationTimer.Stop();
		}

        private void ShowBusy()
        {
            // NOT READY FOR PRIME TIME
//            var mainFrame = Window.Frame;
            //            var origin = new CGPoint(
//                mainFrame.X + ((mainFrame.Width - BusyWindow.Frame.Width) / 2),
//                mainFrame.Y + BusyWindow.Frame.Height);
//            BusyWindow.SetFrameOrigin(origin);
//            BusyWindow.MakeKeyAndOrderFront(this);
        }

        private void HideBusy()
        {
            // NOT READY FOR PRIME TIME
//            BusyWindow.OrderOut(this);
        }

        public void CallWithDelay(Action action, int delay)
        {
            System.Threading.Timer timer = null;
            var cb = new TimerCallback((state) =>
            {
                InvokeOnMainThread(action);
                timer.Dispose();
            });
            timer = new System.Threading.Timer(cb, null, delay, Timeout.Infinite);
        }


		#region IFileViewer implementation

		public bool IsFileSupported(string filePath)
		{
			NSError error;
			var fileType = NSWorkspace.SharedWorkspace.TypeOfFile(filePath, out error);
			return SupportedFileTypes.Contains(fileType);
		}

        public void InvokeOnMainThread(Action action)
        {
            BeginInvokeOnMainThread( () => { action(); } );
        }


		#endregion
	}

	public enum NotificationGraphic
	{
		WrappedToStart,
		WrappedToEnd,
	}

	public enum NsButtonId
	{
		Cancel = 0,
		OK = 1
	}
}
