using Android.Graphics;
using Kit.Droid.Renders;
using Kit.Forms.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Canvas = Android.Graphics.Canvas;
using Context = Android.Content.Context;
using Paint = Android.Graphics.Paint;
using RectF = Android.Graphics.RectF;

[assembly: ExportRenderer(typeof(ProgressRing), typeof(ProgressRingRenderer))]
namespace Kit.Droid.Renders
{
    [Preserve(AllMembers = true)]
    public class ProgressRingRenderer : ViewRenderer
    {
        private Paint _paint;
        private Android.Graphics.RectF _ringDrawArea;
        private bool _sizeChanged = false;


        public ProgressRingRenderer(Context context) : base(context)
        {
            SetWillNotDraw(false);
        }
        protected override void OnDraw(Canvas canvas)
        {
            var progressRing = Element as ProgressRing;

            if (_paint == null)
            {
                var displayDensity = Context.Resources.DisplayMetrics.Density;
                var strokeWidth = (float)Math.Ceiling(progressRing.RingThickness * displayDensity);

                _paint = new Paint();
                _paint.StrokeWidth = strokeWidth;
                _paint.SetStyle(Paint.Style.Stroke);
                _paint.Flags = PaintFlags.AntiAlias;
            }

            if (_ringDrawArea == null || _sizeChanged)
            {
                _sizeChanged = false;

                var ringAreaSize = Math.Min(canvas.ClipBounds.Width(), canvas.ClipBounds.Height());

                var ringDiameter = ringAreaSize - _paint.StrokeWidth;

                var left = canvas.ClipBounds.CenterX() - ringDiameter / 2;
                var top = canvas.ClipBounds.CenterY() - ringDiameter / 2;

                _ringDrawArea = new RectF(left, top, left + ringDiameter, top + ringDiameter);
            }

            var backColor = progressRing.RingBaseColor;
            var frontColor = progressRing.RingProgressColor;
            var progress = (float)progressRing.Progress;
            DrawProgressRing(canvas, progress, backColor, frontColor);
        }

        private void DrawProgressRing(Canvas canvas, float progress,
            Microsoft.Maui.Graphics.Color ringBaseColor,
            Microsoft.Maui.Graphics.Color ringProgressColor)
        {
            _paint.Color = ringBaseColor.ToAndroid();
            canvas.DrawArc(_ringDrawArea, 270, 360, false, _paint);

            _paint.Color = ringProgressColor.ToAndroid();
            canvas.DrawArc(_ringDrawArea, 270, 360 * progress, false, _paint);
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ProgressBar.ProgressProperty.PropertyName ||
                e.PropertyName == ProgressRing.RingThicknessProperty.PropertyName ||
                e.PropertyName == ProgressRing.RingBaseColorProperty.PropertyName ||
                e.PropertyName == ProgressRing.RingProgressColorProperty.PropertyName)
            {
                Invalidate();
            }

            if (e.PropertyName == VisualElement.WidthProperty.PropertyName ||
                e.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                _sizeChanged = true;
                Invalidate();
            }
        }
    }
}