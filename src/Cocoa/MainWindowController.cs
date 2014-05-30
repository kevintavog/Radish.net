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

namespace Radish
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController, IFileViewer
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private const string TrashSoundPath = @"/System/Library/Components/CoreAudio.component/Contents/SharedSupport/SystemSounds/dock/drag to trash.aif";
		static private HashSet<string> SupportedFileTypes = new HashSet<string>(CGImageSource.TypeIdentifiers);


		private DirectoryController		directoryController;
		private string					currentlyDisplayedFile;
		private Timer					hideNotificationTimer = new Timer(250);


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
			directoryController = new DirectoryController(this, FileListUpdated);
			SupportedFileTypes.Add("com.adobe.pdf");
		}

#endregion

		//strongly typed window accessor
		public new MainWindow Window { get { return (MainWindow)base.Window; } }
		public NSWindow NotificationWindow { get { return (NSWindow)notificationWindow; } }
		public FileInformationController InformationController { get { return (FileInformationController)fileInformationController; } }

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.BackgroundColor = NSColor.DarkGray;
			imageView.ImageScaling = NSImageScale.ProportionallyUpOrDown;

			hideNotificationTimer.Elapsed += (s, e) =>
			{
				InvokeOnMainThread( () => HideNotification() );
			};

			imageView.ImageScaling = NSImageScale.ProportionallyDown;
		}

		private void ShowFile()
		{
			if (directoryController.Count < 1)
			{
				currentlyDisplayedFile = null;
				imageView.Image = null;
				Window.Title = "<No files>";
				UpdateStatusBar();
				return;
			}

			var fi = directoryController.Current;
			if (fi.FullPath != currentlyDisplayedFile)
			{
				logger.Info("ShowFile: {0}; {1}", directoryController.CurrentIndex, fi.FullPath);
				currentlyDisplayedFile = fi.FullPath;

				using (var image = new NSImage(fi.FullPath))
				{
					// By getting the image representation & setting the image size, the imageView has
					// good enough info to display a reasonable image. Otherwise, it may use way too
					// small of a representation, not filling out the area it could otherwise.
					var imageRep = image.BestRepresentationForDevice(null);
					image.Size = new SizeF(imageRep.PixelsWide, imageRep.PixelsHigh);
					imageView.Image = image;
					image.Release();
				}
			}

			Window.Title = Path.GetFileName(fi.FullPath);
			UpdateStatusBar();

			if (informationPanel.IsVisible)
			{
				InformationController.SetFile(fi);
			}
		}

		private void UpdateStatusBar()
		{
			if (directoryController.Count < 1)
			{
				statusFilename.StringValue = statusTimestamp.StringValue = statusGps.StringValue = "";
				statusIndex.StringValue = "No files";
				return;
			}

			var fi = directoryController.Current;

			statusFilename.StringValue = Path.GetFileName(fi.FullPath);

			var timestamp = fi.Timestamp.ToString("yyyy/MM/dd HH:mm:ss");
			if (fi.FileAndExifTimestampMatch)
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
				directoryController.CurrentIndex + 1, 
				directoryController.Count);

			statusGps.StringValue = fi.ToDms();
		}

		public bool OpenFolderOrFile(string path)
		{
			string filename = null;
			if (File.Exists(path))
			{
				filename = path;
				path = Path.GetDirectoryName(path);
			}

			logger.Info("Open {0}", path);
			directoryController.Scan(path);
			directoryController.SelectFile(filename);
			ShowFile();

			// That's gross - Mono exposes SharedDocumentController as NSObject rather than NSDocumentcontroller
			(NSDocumentController.SharedDocumentController as NSDocumentController).NoteNewRecentDocumentURL(new NSUrl(path, false));

			return true;
		}

		private void FileListUpdated()
		{
			directoryController.SelectFile(currentlyDisplayedFile);
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
