// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Radish
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.Foundation.NSObject fileInformationController { get; set; }

		[Outlet]
        MonoMac.AppKit.NSImageView imageView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSPanel informationPanel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView notificationImage { get; set; }

		[Outlet]
		MonoMac.Foundation.NSObject notificationWindow { get; set; }

		[Outlet]
		MonoMac.AppKit.NSScrollView scrollView { get; set; }

		[Outlet]
		MonoMac.Foundation.NSObject searchController { get; set; }

		[Outlet]
		MonoMac.AppKit.NSWindow searchWindow { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusFilename { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusGps { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusIndex { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusKeywords { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusTimestamp { get; set; }

		[Outlet]
		MonoMac.AppKit.NSView statusView { get; set; }

		[Outlet]
		MonoMac.Foundation.NSObject thumbController { get; set; }

		[Outlet]
		MonoMac.AppKit.NSWindow thumbnailWindow { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (fileInformationController != null) {
				fileInformationController.Dispose ();
				fileInformationController = null;
			}

			if (informationPanel != null) {
				informationPanel.Dispose ();
				informationPanel = null;
			}

			if (notificationImage != null) {
				notificationImage.Dispose ();
				notificationImage = null;
			}

			if (notificationWindow != null) {
				notificationWindow.Dispose ();
				notificationWindow = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (searchController != null) {
				searchController.Dispose ();
				searchController = null;
			}

			if (searchWindow != null) {
				searchWindow.Dispose ();
				searchWindow = null;
			}

			if (statusFilename != null) {
				statusFilename.Dispose ();
				statusFilename = null;
			}

			if (statusGps != null) {
				statusGps.Dispose ();
				statusGps = null;
			}

			if (statusIndex != null) {
				statusIndex.Dispose ();
				statusIndex = null;
			}

			if (statusKeywords != null) {
				statusKeywords.Dispose ();
				statusKeywords = null;
			}

			if (statusTimestamp != null) {
				statusTimestamp.Dispose ();
				statusTimestamp = null;
			}

			if (statusView != null) {
				statusView.Dispose ();
				statusView = null;
			}

			if (thumbController != null) {
				thumbController.Dispose ();
				thumbController = null;
			}

			if (thumbnailWindow != null) {
				thumbnailWindow.Dispose ();
				thumbnailWindow = null;
			}

			if (imageView != null) {
				imageView.Dispose ();
				imageView = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
