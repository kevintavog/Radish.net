﻿using System;
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


		private MediaListController		mediaListController;
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
			mediaListController = new DirectoryController(this, FileListUpdated);
			SupportedFileTypes.Add("com.adobe.pdf");
		}

#endregion


        public new MainWindow Window { get { return (MainWindow)base.Window; } }
		public NSWindow NotificationWindow { get { return (NSWindow)notificationWindow; } }
		public FileInformationController InformationController { get { return (FileInformationController)fileInformationController; } }
        public SearchController SearchController { get { return (SearchController)searchController; } }

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.BackgroundColor = NSColor.DarkGray;

			hideNotificationTimer.Elapsed += (s, e) =>
			{
				InvokeOnMainThread( () => HideNotification() );
			};

			imageView.ImageScaling = NSImageScale.ProportionallyDown;
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


                // In order to see the current orientation (to see if the image needs to be rotated), load the image 
                // as a CGImage rather than directly via NSImage
                var data = mm.GetData();
                using (var cgImage = CGImage.FromJPEG(new CGDataProvider(data, 0, data.Length), null, false, CGColorRenderingIntent.Default))
                {
                    NSImage image;
                    if (cgImage == null)
                    {
                        image = new NSImage(mm.FullPath);
                    }
                    else
                    {
                        image = new NSImage(cgImage, new SizeF(cgImage.Width, cgImage.Height));
                    }

                    using (image)
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
				statusFilename.StringValue = statusTimestamp.StringValue = statusGps.StringValue = "";
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

			statusGps.StringValue = mm.ToDms();
		}

		public bool OpenFolderOrFile(string path)
		{
			string filename = null;
			if (File.Exists(path))
			{
				filename = path;
				path = Path.GetDirectoryName(path);
			}

            var dirController = mediaListController as DirectoryController;
            if (dirController == null)
            {
                dirController = new DirectoryController(this, FileListUpdated);
                mediaListController = dirController as MediaListController;
            }
			logger.Info("Open {0}", path);
			dirController.Scan(path);
			mediaListController.SelectFile(filename);
			ShowFile();

			// That's gross - Mono exposes SharedDocumentController as NSObject rather than NSDocumentcontroller
			(NSDocumentController.SharedDocumentController as NSDocumentController).NoteNewRecentDocumentURL(new NSUrl(path, false));

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
