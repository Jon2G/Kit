namespace Kit.Controls.CrossBrush
{
    public abstract class Color
    {
        public abstract float R { get; set; }
        public abstract float G { get; set; }
        public abstract float B { get; set; }
        public abstract float A { get; set; }
        public static string Transparent => "#00FFFFFF";


        public Color() { }
        public Color(float R, float G, float B, float A)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }

        public abstract Color From(string v);
        public abstract object ToNativeColor();

        public override string ToString()
        {
            return ToHex();
        }
        public string ToHex()
        {
            return $"#{(int)R:X2}{(int)G:X2}{(int)B:X2}";
        }
    }
}
