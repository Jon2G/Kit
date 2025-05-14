using System.Collections.ObjectModel;

namespace Kit.Controls.CrossBrush
{
    public sealed class GradientStopCollection<C> : ObservableCollection<GradientStop<C>> where C : Color, new()
    {

    }
}
