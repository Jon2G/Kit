﻿
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;

namespace Kit.Forms.Controls.ActivityIndicator
{
    internal class FourArcs : ArcActivityIndicator
    {


        float OvalStartAngle = 90; //outer arc start angle
        float SecondOvalStartAngle = 270; //outer arc start angle
        float OvalSweepAngle = 80; //outer arcg sweep angle from the start angle position


        float InnerOvalStartAngle = 90; //inner arc start angle
        float InnerSecondOvalStartAngle = 270; //inner arc start angle
        float InnerOvalSweepAngle = 80; //inner arcg sweep angle from the start angle position

        /// <summary>
        /// outer arc paint style
        /// defined the style as stroke
        /// color of the stroke using hsl format * you can change the color code here*
        /// width of the outer arc
        /// </summary>
        SKPaint firstArcPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,

            Color = SKColor.FromHsl(293, 44, 47),
            StrokeWidth = 25,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round

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

            Color = SKColor.FromHsl(294, 44, 75),
            StrokeWidth = 25,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round

        };

        public FourArcs() : base()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerClik);
        }
        public bool OnTimerClik()
        {
            OvalStartAngle += 2;
            SecondOvalStartAngle += 2;
            InnerOvalStartAngle += 8;
            InnerSecondOvalStartAngle += 8;
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
            SKRect InnerRect = new SKRect(left + 100, top + 100, (info.Width - right) - 100, (info.Height - bottom) - 100);
            //canvas.DrawCircle(info.Width / 2, info.Height / 2, 250, firstArcPaint);
            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, OvalStartAngle, OvalSweepAngle);
                canvas.DrawPath(path, firstArcPaint);
            }

            using (SKPath path = new SKPath())
            {
                path.AddArc(rect, SecondOvalStartAngle, OvalSweepAngle);
                canvas.DrawPath(path, firstArcPaint);
            }

            using (SKPath path = new SKPath())
            {
                path.AddArc(InnerRect, InnerOvalStartAngle, InnerOvalSweepAngle);
                canvas.DrawPath(path, secondArcPaint);
            }

            using (SKPath path = new SKPath())
            {
                path.AddArc(InnerRect, InnerSecondOvalStartAngle, InnerOvalSweepAngle);
                canvas.DrawPath(path, secondArcPaint);
            }
        }
    }
}
