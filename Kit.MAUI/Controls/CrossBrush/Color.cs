namespace Kit.Controls.CrossBrush
{
    public abstract class Color
    {
        public abstract double R { get; set; }
        public abstract double G { get; set; }
        public abstract double B { get; set; }
        public abstract double A { get; set; }
        public static string Transparent => "#00FFFFFF";


        public Color() { }
        public Color(double R, double G, double B, double A)
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
