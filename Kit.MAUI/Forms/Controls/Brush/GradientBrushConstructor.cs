using Kit.Controls.CrossBrush;

namespace Kit.Forms.Controls.Brush
{
    public static class GradientBrushConstructor
    {
        public static CustomBackground
            <Microsoft.Maui.Controls.Brush,
                Kit.Forms.Controls.Brush.Brush,
                Kit.Forms.Controls.Brush.Color>
            Get<T>
            (
                string MainBackround = "MainBackround",
                string DarkBackround = "DarkBackround")
            where T : CustomBackground<Microsoft.Maui.Controls.Brush,
                Kit.Forms.Controls.Brush.Brush,
                Kit.Forms.Controls.Brush.Color>
        {
            return CustomBackground<Microsoft.Maui.Controls.Brush,
                    Kit.Forms.Controls.Brush.Brush,
                    Kit.Forms.Controls.Brush.Color>
                .Get<T>(MainBackround, DarkBackround);
        }
    }
}
