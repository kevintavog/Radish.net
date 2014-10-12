using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.CoreText;
using MonoMac.Foundation;
using MonoMac.ImageIO;
using NLog;
using Radish.Controllers;
using Radish.Models;
using System.Threading.Tasks;
using MonoMac.ObjCRuntime;

namespace Radish
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController, IFileViewer
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private const string TrashSoundPath = @"/System/Library/Components/CoreAudio.component/Contents/SharedSupport/SystemSounds/dock/drag to trash.aif";
		static private HashSet<string> SupportedFileTypes = new HashSet<string>(CGImageSource.TypeIdentifiers);


		private MediaListController		mediaListController;
		private string					currentlyDisplayedFile;
		private Timer					hideNotificationTimer = new Timer(250);
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
		public FileInformationController InformationController { get { return (FileInformationController)fileInformationController; } }
        public SearchController SearchController { get { return (SearchController)searchController; } }
        public ThumbController ThumbController { get { return (ThumbController)thumbController; } }

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

            zoomView = new ZoomView(imageView);
			Window.BackgroundColor = NSColor.DarkGray;
            NSApplication.CheckForIllegalCrossThreadCalls = false;

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
                        // In order to see the current orientation (to see if the image needs to be rotated), load the image 
                        // as a CGImage rather than directly via NSImage
                        var data = mm.GetData();

                        if (index != mediaListController.CurrentIndex)
                        {
                            logger.Info("Ignoring load of outdated image: {0}", index);
                            return;
                        }

                        using (var cgImage = CGImage.FromJPEG(new CGDataProvider(data, 0, data.Length), null, false, CGColorRenderingIntent.Default))
                        {
                            NSImage image;
                            if (cgImage == null)
                            {
                                // It's not a JPEG, or at least can't be loaded that way.
                                image = new NSImage(NSData.FromArray(data));
                            }
                            else
                            {
                                image = new NSImage(cgImage, new SizeF(cgImage.Width, cgImage.Height));
                            }

                            imageView.Image = image;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Exception loading & displaying image: {0}", e);
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

        private void SetStatusGps(MediaMetadata mm)
        {
            if (!String.IsNullOrEmpty(mm.ToPlaceName()))
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

		#region IFileViewer implementation

		public void InvokeOnMainThread(Action action)
		{
			BeginInvokeOnMainThread( () => { action(); } );
		}

		public bool IsFileSupported(string filePath)
		{
			NSError error;
			var fileType = NSWorkspace.SharedWorkspace.TypeOfFile(filePath, out error);
			return SupportedFileTypes.Contains(fileType);
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
