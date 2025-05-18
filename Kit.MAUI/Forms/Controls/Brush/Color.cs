namespace Kit.Forms.Controls.Brush
{
    public class Color : Kit.Controls.CrossBrush.Color
    {
        private Microsoft.Maui.Graphics.Color NativeColor;
        public override float R
        {
            get => NativeColor.Red;
            set => NativeColor = new Microsoft.Maui.Graphics.Color(value, G, B, A);
        }

        public override float G
        {
            get => NativeColor.Green;
            set => NativeColor = new Microsoft.Maui.Graphics.Color(R, value, B, A);

        }
        public override float B
        {
            get => NativeColor.Blue;
            set => NativeColor = new Microsoft.Maui.Graphics.Color(R, G, value, A);

        }
        public override float A
        {
            get => NativeColor.Alpha;
            set => NativeColor = new Microsoft.Maui.Graphics.Color(R, G, B, value);
        }
        public override Kit.Controls.CrossBrush.Color From(string v)
        {
            return new Color()
            {
                NativeColor = Microsoft.Maui.Graphics.Color.FromHex(v)
            };
        }

        public override object ToNativeColor()
        {
            return NativeColor;
        }
    }
}
