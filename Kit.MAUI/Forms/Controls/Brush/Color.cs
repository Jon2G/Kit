namespace Kit.Forms.Controls.Brush
{
    public class Color : Kit.Controls.CrossBrush.Color
    {
        private Xamarin.Forms.Color NativeColor;
        public override double R
        {
            get => NativeColor.R;
            set => NativeColor = new Xamarin.Forms.Color(value, G, B, A);
        }

        public override double G
        {
            get => NativeColor.G;
            set => NativeColor = new Xamarin.Forms.Color(R, value, B, A);

        }
        public override double B
        {
            get => NativeColor.B;
            set => NativeColor = new Xamarin.Forms.Color(R, G, value, A);

        }
        public override double A
        {
            get => NativeColor.A;
            set => NativeColor = new Xamarin.Forms.Color(R, G, B, value);
        }
        public override Kit.Controls.CrossBrush.Color From(string v)
        {
            return new Color()
            {
                NativeColor = Xamarin.Forms.Color.FromHex(v)
            };
        }

        public override object ToNativeColor()
        {
            return NativeColor;
        }
    }
}
