using System;
using Kit.Forms.Controls.ActivityIndicator;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

// ReSharper disable once CheckNamespace
namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityIndicatorView : ContentView
    {
        public static readonly BindableProperty IndicatorProperty = BindableProperty.Create(
            propertyName: nameof(Indicator),
            returnType: typeof(IndicatorType),
            declaringType: typeof(ActivityIndicatorView),
            defaultValue: IndicatorType.None,
            propertyChanged: OnPropertyChanged,
            defaultBindingMode: BindingMode.TwoWay);

        private static void OnPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is ActivityIndicatorView view)
            {
                view.Indicator = (IndicatorType)(newvalue);
            }
        }

        public IndicatorType Indicator
        {
            get => (IndicatorType)GetValue(IndicatorProperty);
            set
            {
                SetValue(IndicatorProperty, value);
                OnPropertyChanged();
                ChangeIndicator();
            }
        }

        public ActivityIndicatorView()
        {
            InitializeComponent();
        }

        private void ChangeIndicator()
        {
            switch (this.Indicator)
            {
                case IndicatorType.ArcWithinArc:
                    this.Content = new ArcWithinArc();
                    break;
                case IndicatorType.FourArcs:
                    this.Content = new FourArcs();
                    break;
                case IndicatorType.OneArc:
                    this.Content = new OneArc();
                    break;
                case IndicatorType.ThreeArcs:
                    this.Content = new ThreeArcs();
                    break;
                case IndicatorType.ThreeArcsWithTwoInSamePosition:
                    this.Content = new ThreeArcsWithTwoInSamePosition();
                    break;
                case IndicatorType.TwoArcs:
                    this.Content = new TwoArcs();
                    break;
                case IndicatorType.TwoSepareteArcs:
                    this.Content = new TwoSepareteArcs();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Indicator), Indicator, null);
            }
        }
    }
}