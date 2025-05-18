using Kit.Controls.CrossBrush;

namespace Kit.Forms.Controls.Brush
{
    public class Brush : CrossBrush<Microsoft.Maui.Controls.Brush, Color>
    {
        public override Microsoft.Maui.Controls.Brush ToNaviteBrush()
        {
            Microsoft.Maui.Controls.Brush native = null;
            switch (this.BrushType)
            {
                case BrushType.Solid:
                    native = new SolidColorBrush();
                    if (this.Stops.Any())
                    {
                        ((SolidColorBrush)native).Color =
                            (Microsoft.Maui.Graphics.Color)this.Stops.First().Color.ToNativeColor();
                    }

                    return native;

                case BrushType.Radial:
                    native = new RadialGradientBrush()
                    {
                        Radius = (new List<double>() { RadiusX, RadiusY }).Max()
                    };
                    break;

                case BrushType.Linear:
                    native = new LinearGradientBrush();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (native is GradientBrush g)
            {
                CopyStops(g, this.Stops);
            }
            return native;
        }

        private void CopyStops(GradientBrush native, GradientStopCollection<Color> stops)
        {
            foreach (var stop in stops)
            {
                var Color = (Microsoft.Maui.Graphics.Color)stop.Color.ToNativeColor();
                native.GradientStops.Add(new GradientStop(Color, stop.Offset));
            }
        }

        public override void Apply()
        {
            Application.Current.Resources[ResourceKey] = this.ToNaviteBrush();
        }

        public override void LoadFromResource()
        {
            this.Stops.Clear();
            if (Application.Current.Resources[ResourceKey] is SolidColorBrush s)
            {
                var c = s.Color;
                string color = $"#{c.Red:X2}{c.Green:X2}{c.Blue:X2}";
                this.Stops.Add(new GradientStop<Color>(new Color().From(color), 0));
                this.BrushType = BrushType.Solid;
            }
            if (Application.Current.Resources[ResourceKey] is GradientBrush g)
            {
                foreach (var stop in g.GradientStops)
                {
                    var c = stop.Color;
                    string color = stop.Color.ToHex();
                    this.Stops.Add(new GradientStop<Color>(new Color().From(color), (float)stop.Offset));
                }

                switch (g)
                {
                    case LinearGradientBrush:
                        this.BrushType = BrushType.Linear;
                        break;

                    case RadialGradientBrush r:
                        this.RadiusX = r.Radius;
                        this.RadiusY = r.Radius;
                        this.BrushType = BrushType.Radial;
                        break;
                }
            }
            return;
        }
    }
}