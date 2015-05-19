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
	partial class SearchController
	{
		[Outlet]
        MonoMac.AppKit.NSImageView connectionImage { get; set; }

		[Outlet]
        MonoMac.AppKit.NSTextField errorLabel { get; set; }

		[Outlet]
        MonoMac.AppKit.NSTextField hostName { get; set; }

		[Outlet]
        MonoMac.AppKit.NSProgressIndicator progressIndicator { get; set; }

		[Outlet]
        MonoMac.AppKit.NSButton searchButton { get; set; }

		[Outlet]
        MonoMac.AppKit.NSTextField searchText { get; set; }

		[Outlet]
        MonoMac.AppKit.NSButton testButton { get; set; }

		[Action ("cancel:")]
        partial void cancel (MonoMac.Foundation.NSObject sender);

		[Action ("startSearch:")]
        partial void startSearch (MonoMac.Foundation.NSObject sender);

		[Action ("testHost:")]
        partial void testHost (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (connectionImage != null) {
				connectionImage.Dispose ();
				connectionImage = null;
			}

			if (errorLabel != null) {
				errorLabel.Dispose ();
				errorLabel = null;
			}

			if (hostName != null) {
				hostName.Dispose ();
				hostName = null;
			}

			if (progressIndicator != null) {
				progressIndicator.Dispose ();
				progressIndicator = null;
			}

			if (searchButton != null) {
				searchButton.Dispose ();
				searchButton = null;
			}

			if (searchText != null) {
				searchText.Dispose ();
				searchText = null;
			}

			if (testButton != null) {
				testButton.Dispose ();
				testButton = null;
			}
		}
	}
}
