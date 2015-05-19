using System;
using MonoMac.AppKit;
using System.Drawing;

namespace Radish
{
    public class ZoomView
    {
        public float MinimumScale { get; set; }
        public float MaximumScale { get; set; }


        private readonly NSView view;
        private float viewScale = 1.0f;


        public ZoomView(NSView view)
        {
            this.view = view;
            MinimumScale = 1;
            MaximumScale = 25;
        }

        public void ZoomIn()
        {
            ZoomViewByFactor(2.0f);
        }

        public void ZoomOut()
        {
            ZoomViewByFactor(0.5f);
        }

        public void ZoomToActualSize()
        {
            ZoomViewByFactor(1.0f / viewScale);
        }

        public void ZoomToFit()
        {
            ZoomViewToFitRect(view.Superview.Frame);
        }

        public void ZoomViewToFitRect(RectangleF rect)
        {
            var frame = view.Frame;
            var factor = (rect.Width / frame.Width);
            factor = Math.Min(factor, (rect.Height / frame.Height));
            ZoomViewByFactor(factor);
        }

        private void ZoomViewByFactor(float factor)
        {
            ZoomViewByFactor(factor, DocumentCenterPoint());
        }

        private void ZoomViewByFactor(float factor, PointF point)
        {
            var scale = factor * viewScale;
            scale = Math.Max(scale, MinimumScale);
            scale = Math.Min(scale, MaximumScale);

            if (Math.Abs(scale - viewScale) > 0.0001)
            {
                viewScale = scale;
                view.ScaleUnitSquareToSize(new SizeF { Width = factor, Height = factor });

                var frame = view.Frame;
                frame.Width *= factor;
                frame.Height *= factor;
                view.Frame = frame;

                ScrollPointToCenter(point);
                view.NeedsDisplay = true;
            }
        }

        public PointF DocumentCenterPoint()
        {
            var frame = ((NSClipView)(view.Superview)).DocumentVisibleRect();
            return new PointF
            {
                X = frame.X + frame.Width / 2,
                Y = frame.Y + frame.Height / 2,
            };
        }

        public void ScrollPointToCenter(PointF point)
        {
            var frame = ((NSClipView)(view.Superview)).DocumentVisibleRect();
            view.ScrollPoint(new PointF
            {
                X = point.X - (frame.Width / 2),
                Y = point.Y - (frame.Height / 2)
            });
        }
    }
}
