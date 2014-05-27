using System;
using MonoMac.AppKit;
using System.Drawing;
using MonoMac.Foundation;

namespace Radish
{
	[MonoMac.Foundation.Register("KTBorderlessWindow")]
	public class KTBorderlessWindow : NSWindow
	{
		[Export("initWithContentRect:styleMask:backing:defer:")]
		public KTBorderlessWindow(RectangleF contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation)
			: base(contentRect, aStyle, bufferingType, deferCreation)
		{
			BackgroundColor = NSColor.Clear;
			AlphaValue = 0.50f;
			IsOpaque = false;
			Level = NSWindowLevel.Floating;
		}
	}
}
