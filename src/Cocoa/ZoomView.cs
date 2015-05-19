using System;
using AppKit;
using System.Drawing;
using NLog;
using CoreGraphics;

namespace Radish
{
    public class ZoomView
    {
        static private readonly Logger logger = LogManager.GetCurrentClassLogger();


        public float MinimumScale { get; set; }
        public float MaximumScale { get; set; }


        private NSView view;
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

        public void ZoomViewToFitRect(CGRect rect)
        {
            var frame = view.Frame;
            var factor = (float) (rect.Width / frame.Width);
            factor = Math.Min((float)factor, (float) (rect.Height / frame.Height));
            ZoomViewByFactor(factor);
        }

        private void ZoomViewByFactor(float factor)
        {
            ZoomViewByFactor(factor, DocumentCenterPoint());
        }

        private void ZoomViewByFactor(float factor, CGPoint point)
        {
            var scale = factor * viewScale;
            scale = Math.Max(scale, MinimumScale);
            scale = Math.Min(scale, MaximumScale);
logger.Info("ZoomViewByFactor({0}); current scale = {1}, target scale = {2}", factor, viewScale, scale);

            if (scale != viewScale)
            {
                viewScale = scale;
                view.ScaleUnitSquareToSize(new CGSize { Width = factor, Height = factor });

                var frame = view.Frame;
                frame.Width *= factor;
                frame.Height *= factor;
                view.Frame = frame;

                ScrollPointToCenter(point);
                view.NeedsDisplay = true;
            }
        }

        public CGPoint DocumentCenterPoint()
        {
            var frame = ((NSClipView)(view.Superview)).DocumentVisibleRect();
            return new CGPoint
            {
                X = frame.X + frame.Width / 2,
                Y = frame.Y + frame.Height / 2,
            };
        }

        public void ScrollPointToCenter(CGPoint point)
        {
            var frame = ((NSClipView)(view.Superview)).DocumentVisibleRect();
            view.ScrollPoint(new CGPoint
            {
                X = point.X - (frame.Width / 2),
                Y = point.Y - (frame.Height / 2)
            });
        }
    }
}
