namespace Kit.Controls.CrossBrush
{
    public class GradientStop<C> where C : Color,new()
    {
        public Color Color { get; set; }
        public float Offset { get; set; }

        public GradientStop(Color Color, float Offset)
        {
            this.Color = Color;
            this.Offset = Offset;
        }
        public GradientStop<C> Oscurecer(double CorrectionFactor = -0.3)
        {
            double red = (double)Color.R;
            double green = (double)Color.G;
            double blue = (double)Color.B;

            if (CorrectionFactor < 0)
            {
                CorrectionFactor = 1 + CorrectionFactor;
                red *= CorrectionFactor;
                green *= CorrectionFactor;
                blue *= CorrectionFactor;
            }
            else
            {
                red = (255 - red) * CorrectionFactor + red;
                green = (255 - green) * CorrectionFactor + green;
                blue = (255 - blue) * CorrectionFactor + blue;
            }

            return new GradientStop<C>(new C()
            {
                A = Color.A,
                R = red,
                G = green,
                B = blue,

            }, (float)Color.A)
            {
                Offset = Offset
            };

        }
    }
}
