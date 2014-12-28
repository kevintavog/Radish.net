using System;
using AppKit;
using System.Drawing;
using Foundation;
using CoreGraphics;

namespace Radish
{
	[Foundation.Register("KTBorderlessWindow")]
	public class KTBorderlessWindow : NSWindow
	{
		[Export("initWithContentRect:styleMask:backing:defer:")]
		public KTBorderlessWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation)
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
			BackgroundColor = NSColor.Clear;
			AlphaValue = 0.50f;
			IsOpaque = false;
			Level = NSWindowLevel.Floating;
		}
	}
}
