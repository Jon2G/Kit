using System;
using System.Collections.Generic;
using System.Linq;
using Kit.Controls.CrossBrush;
using Xamarin.Forms;

namespace Kit.Forms.Controls.Brush
{
    public class Brush : CrossBrush<Xamarin.Forms.Brush, Color>
    {
        public override Xamarin.Forms.Brush ToNaviteBrush()
        {
            Xamarin.Forms.Brush native = null;
            switch (this.BrushType)
            {
                case BrushType.Solid:
                    native = new SolidColorBrush();
                    if (this.Stops.Any())
                    {
                        ((SolidColorBrush)native).Color =
                            (Xamarin.Forms.Color)this.Stops.First().Color.ToNativeColor();
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
                var Color = (Xamarin.Forms.Color)stop.Color.ToNativeColor();
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
                string color = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
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