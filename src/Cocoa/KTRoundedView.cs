using System;
using MonoMac.AppKit;
using System.Drawing;

namespace Radish
{
    [MonoMac.Foundation.Register("KTRoundedView")]
	public class KTRoundedView : NSView
	{
        public NSColor BackgroundColor { get; set; }

        public KTRoundedView(IntPtr handle) : base(handle) 
        {
            CommonInit();
        }

        public KTRoundedView(RectangleF frameRect) : base(frameRect)
		{
            CommonInit();
		}

        private void CommonInit()
        {
            BackgroundColor = NSColor.Black;
        }

        public override void DrawRect(RectangleF dirtyRect)
		{
			var path = NSBezierPath.FromRoundedRect(dirtyRect, 6.0f, 6.0f);
            BackgroundColor.Set();
			path.Fill();
		}
	}
}

