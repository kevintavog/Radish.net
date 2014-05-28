using System;
using MonoMac.AppKit;
using System.Drawing;

namespace Radish
{
	[MonoMac.Foundation.Register("KTRoundedView")]
	public class KTRoundedView : NSView
	{
		public KTRoundedView(IntPtr handle) : base(handle) {}
		public KTRoundedView(RectangleF frameRect) : base(frameRect)
		{
		}

		public override void DrawRect(RectangleF dirtyRect)
		{
			var path = NSBezierPath.FromRoundedRect(dirtyRect, 6.0f, 6.0f);
			NSColor.Black.Set();
			path.Fill();
		}
	}
}

