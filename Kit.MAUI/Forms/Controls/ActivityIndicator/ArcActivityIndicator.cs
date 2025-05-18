using System.Diagnostics;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Kit.Forms.Controls.ActivityIndicator
{
    internal abstract class ArcActivityIndicator : ContentView
    {
        protected readonly SKCanvasView Canvas;
        //stopwatch to start the timer to render the animation
        protected readonly Stopwatch Stopwatch;
        protected abstract void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args);

        protected virtual SKCanvasView BuildCanvas()
        {
            var canvas = new SKCanvasView();
            canvas.PaintSurface += OnCanvasViewPaintSurface;
            return canvas;
        }

        public ArcActivityIndicator()
        {
            this.Stopwatch = new Stopwatch();
            this.Canvas = BuildCanvas();
            this.Content = this.Canvas;
        }
    }
}
