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
		MonoMac.AppKit.NSImageView imageView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusFilename { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusGps { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusIndex { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField statusTimestamp { get; set; }

		[Outlet]
		MonoMac.AppKit.NSView statusView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imageView != null) {
				imageView.Dispose ();
				imageView = null;
			}

			if (statusView != null) {
				statusView.Dispose ();
				statusView = null;
			}

			if (statusFilename != null) {
				statusFilename.Dispose ();
				statusFilename = null;
			}

			if (statusTimestamp != null) {
				statusTimestamp.Dispose ();
				statusTimestamp = null;
			}

			if (statusIndex != null) {
				statusIndex.Dispose ();
				statusIndex = null;
			}

			if (statusGps != null) {
				statusGps.Dispose ();
				statusGps = null;
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
