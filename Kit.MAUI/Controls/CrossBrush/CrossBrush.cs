namespace Kit.Controls.CrossBrush
{
    public abstract class CrossBrush<B, C>
    where C : Color, new()
    {
        public BrushType BrushType { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public GradientStopCollection<C> Stops { get; set; }
        public string ResourceKey { get; set; }

        public CrossBrush(string ResourceName)
        {
            this.ResourceKey = ResourceName;
            this.Stops = new GradientStopCollection<C>();
        }
        protected CrossBrush()
        {
            this.Stops = new GradientStopCollection<C>();
        }
        public abstract B ToNaviteBrush();
        public abstract void Apply();
        public abstract void LoadFromResource();

        public CrossBrush<B, C> Darken(CrossBrush<B, C> Brush, double correctionFactor = -0.3)
        {
            for (int i = 0; i < Brush.Stops.Count; i++)
            {
                Brush.Stops[i] = Brush.Stops[i].Oscurecer(correctionFactor);
            }

            Brush.BrushType = this.BrushType;
            return Brush;
        }

    }
}
