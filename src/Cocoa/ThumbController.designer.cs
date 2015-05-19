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
	partial class ThumbController
	{
		[Outlet]
        MonoMac.AppKit.NSSlider imageSizeSlider { get; set; }

		[Outlet]
        MonoMac.ImageKit.IKImageBrowserView imageView { get; set; }

		[Action ("UpdateThumbSize:")]
        partial void UpdateThumbSize (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (imageSizeSlider != null) {
				imageSizeSlider.Dispose ();
				imageSizeSlider = null;
			}

			if (imageView != null) {
				imageView.Dispose ();
				imageView = null;
			}
		}
	}
}
