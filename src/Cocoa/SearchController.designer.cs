// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Radish
{
	partial class SearchController
	{
		[Outlet]
		AppKit.NSImageView connectionImage { get; set; }

		[Outlet]
		AppKit.NSTextField errorLabel { get; set; }

		[Outlet]
		AppKit.NSTextField hostName { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator progressIndicator { get; set; }

		[Outlet]
		AppKit.NSButton searchButton { get; set; }

		[Outlet]
		AppKit.NSTextField searchText { get; set; }

		[Outlet]
		AppKit.NSButton testButton { get; set; }

		[Action ("cancel:")]
		partial void cancel (Foundation.NSObject sender);

		[Action ("startSearch:")]
		partial void startSearch (Foundation.NSObject sender);

		[Action ("testHost:")]
		partial void testHost (Foundation.NSObject sender);
		
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
