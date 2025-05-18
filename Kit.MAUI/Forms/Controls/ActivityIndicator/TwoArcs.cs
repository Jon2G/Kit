
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;
namespace Kit.Forms.Controls.ActivityIndicator
{
    internal class TwoArcs : ArcActivityIndicator
    {


        float OvalStartAngle = 90; //outer arc start angle
        float SecondOvalStartAngle = 270; //outer arc start angle
        float OvalSweepAngle = 50; //outer arcg sweep angle from the start angle position

        /// <summary>
        /// outer arc paint style
        /// defined the style as stroke
        /// color of the stroke using hsl format * you can change the color code here*
        /// width of the outer arc
        /// </summary>
        SKPaint firstArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,

            Color = SKColor.FromHsl(240, 8, 66),
            StrokeWidth = 25,
            IsAntialias = true

        };

        /// <summary>
        /// middle arc paint style
        /// defined the style as stroke
        /// color of the stroke using hsl format * you can change the color code here*
        /// width of the middle arc
        /// </summary>
        SKPaint secondArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,

            Color = SKColor.FromHsl(341, 100, 45),
            StrokeWidth = 25,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round

        };
        public TwoArcs() : base()
        {
            Stopwatch.Start();
            Device.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerClik);
        }

        public bool OnTimerClik()
        {
            OvalStartAngle += 5;
            SecondOvalStartAngle += 5;
            Canvas.InvalidateSurface();
            return true;
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

            canvas.DrawCircle(info.Width / 2, info.Height / 2, 250, firstArcPaint);
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, OvalStartAngle, OvalSweepAngle);
                canvas.DrawPath(path, secondArcPaint);
            }
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, SecondOvalStartAngle, OvalSweepAngle);
                canvas.DrawPath(path, secondArcPaint);
            }
        }
    }
}
