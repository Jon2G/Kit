using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;

namespace Kit.Forms.Controls.ActivityIndicator
{
    internal class TwoSepareteArcs : ArcActivityIndicator
    {

        float OvalStartAngle = 90; //outer arc start angle
        float OvalSweepAngle = 180; //outer arcg sweep angle from the start angle position
        float InnerOvalStartAngle = 90; //inner arc start angle
        float InnerOvalSweepAngle = 180; //inner arcg sweep angle from the start angle position

        /// <summary>
        /// outer arc paint style
        /// defined the style as stroke
        /// color of the stroke using hsl format * you can change the color code here*
        /// width of the outer arc
        /// </summary>
        SKPaint firstArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,

            Color = SKColor.FromHsl(216, 92, 73),
            StrokeWidth = 25,
            IsAntialias = true

        };

        /// <summary>
        /// inner arc paint style
        /// defined the style as stroke
        /// color of the stroke using hsl format * you can change the color code here*
        /// width of the inner arc
        /// </summary>
        SKPaint secondArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,

            Color = SKColor.FromHsl(216, 69, 78),
            StrokeWidth = 25,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round

        };
        public bool OnTimerClik()
        {
            OvalStartAngle += 5;
            InnerOvalStartAngle -= 5;
            Canvas.InvalidateSurface();
            return true;
        }
        public TwoSepareteArcs()
        {
            Stopwatch.Start();
            Device.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerClik);
        }
        protected override void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            /*
             * 500 is the diameter of the outer arc
             * each inner arc from the outer arc will get reduced by 50
             *you can change the value according to make the arc smaller or bigger
             * */

            float left, right;
            float top, bottom;
            right = left = (info.Width - 500) / 2; //get the left and right postions to support all the devices
            top = bottom = (info.Height - 500) / 2;//get the top and bottom postions to support all the devices

            //first Arc
            SKRect rect = new SKRect(left, top, info.Width - right, info.Height - bottom);


            SKRect innerRect = new SKRect(left + 100, top + 100, (info.Width - right) - 100, (info.Height - bottom) - 100);
            //canvas.DrawCircle(info.Width / 2, info.Height / 2, 250, firstArcPaint);
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, OvalStartAngle, OvalSweepAngle);
                canvas.DrawPath(path, firstArcPaint);
            }

            using (SKPath path = new SKPath())
            {
                path.AddArc(innerRect, InnerOvalStartAngle, InnerOvalSweepAngle);
                canvas.DrawPath(path, secondArcPaint);
            }
        }
    }
}
