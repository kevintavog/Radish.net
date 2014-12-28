using System;
using AppKit;
using System.Drawing;
using CoreGraphics;

namespace Radish
{
	[Foundation.Register("KTRoundedView")]
	public class KTRoundedView : NSView
	{
        public NSColor BackgroundColor { get; set; }

        public KTRoundedView(IntPtr handle) : base(handle) 
        {
            CommonInit();
        }

        public KTRoundedView(CGRect frameRect) : base(frameRect)
		{
            CommonInit();
		}

        private void CommonInit()
        {
            BackgroundColor = NSColor.Black;
        }

		public override void DrawRect(CGRect dirtyRect)
		{
			var path = NSBezierPath.FromRoundedRect(dirtyRect, 6.0f, 6.0f);
            BackgroundColor.Set();
			path.Fill();
		}
	}
}

