
using Kit.Forms.Controls;
using Kit.iOS.Renders;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportRenderer(typeof(CircleView), typeof(CircleViewRenderer))]
namespace Kit.iOS.Renders
{
    public class CircleViewRenderer : BoxRenderer
    {
        public static void Initialize() { }
        protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
                return;

            Layer.MasksToBounds = true;
            Layer.CornerRadius = (float)((CircleView)Element).BadgeCornerRadius / 2.0f;

        }

    }
}