using System.Windows.Input;

namespace Kit.MAUI.Forms.Pages
{
    public class ZXingDefaultOverlay : Grid
    {
        readonly Label topText;
        readonly Label botText;
        readonly Button flash;

        public delegate void FlashButtonClickedDelegate(Button sender, EventArgs e);
        public event FlashButtonClickedDelegate FlashButtonClicked;

        public ZXingDefaultOverlay()
        {
            BindingContext = this;

            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;

            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            var b1 = new BoxView()
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Black,
                Opacity = 0.7,
            };
            Grid.SetColumn(b1, 0);
            Grid.SetRow(b1, 0);
            Children.Add(b1);

            var b2 = new BoxView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.Black,
                Opacity = 0.7,
            };
            Grid.SetColumn(b2, 0);
            Grid.SetRow(b2, 2);
            Children.Add(b2);

            var b3 = new BoxView
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 3,
                BackgroundColor = Colors.Red,
                Opacity = 0.6,
            };
            Grid.SetColumn(b3, 0);
            Grid.SetRow(b3, 1);
            Children.Add(b3);
            Children.Add(b3);

            topText = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.White,
                AutomationId = "zxingDefaultOverlay_TopTextLabel",
            };
            topText.SetBinding(Label.TextProperty, new Binding(nameof(TopText)));
            Children.Add(topText);

            botText = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.White,
                AutomationId = "zxingDefaultOverlay_BottomTextLabel",
            };
            botText.SetBinding(Label.TextProperty, new Binding(nameof(BottomText)));
            Grid.SetColumn(botText, 0);
            Grid.SetRow(botText, 2);
            Children.Add(botText);

            flash = new Button
            {
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Text = "Flash",
                TextColor = Colors.White,
                AutomationId = "zxingDefaultOverlay_FlashButton",
            };
            flash.SetBinding(Button.IsVisibleProperty, new Binding(nameof(ShowFlashButton)));
            flash.Clicked += (sender, e) =>
            {
                FlashButtonClicked?.Invoke(flash, e);
            };

            Children.Add(flash);
        }

        public static readonly BindableProperty TopTextProperty =
            BindableProperty.Create(nameof(TopText), typeof(string), typeof(ZXingDefaultOverlay), string.Empty);
        public string TopText
        {
            get => (string)GetValue(TopTextProperty);
            set => SetValue(TopTextProperty, value);
        }

        public static readonly BindableProperty BottomTextProperty =
            BindableProperty.Create(nameof(BottomText), typeof(string), typeof(ZXingDefaultOverlay), string.Empty);
        public string BottomText
        {
            get => (string)GetValue(BottomTextProperty);
            set => SetValue(BottomTextProperty, value);
        }

        public static readonly BindableProperty ShowFlashButtonProperty =
            BindableProperty.Create(nameof(ShowFlashButton), typeof(bool), typeof(ZXingDefaultOverlay), false);
        public bool ShowFlashButton
        {
            get => (bool)GetValue(ShowFlashButtonProperty);
            set => SetValue(ShowFlashButtonProperty, value);
        }

        public static BindableProperty FlashCommandProperty =
            BindableProperty.Create(nameof(FlashCommand), typeof(ICommand), typeof(ZXingDefaultOverlay),
                defaultValue: default(ICommand),
                propertyChanged: OnFlashCommandChanged);

        public ICommand FlashCommand
        {
            get => (ICommand)GetValue(FlashCommandProperty);
            set => SetValue(FlashCommandProperty, value);
        }

        static void OnFlashCommandChanged(BindableObject bindable, object oldvalue, object newValue)
        {
            var overlay = bindable as ZXingDefaultOverlay;
            if (overlay?.flash == null)
                return;
            overlay.flash.Command = newValue as Command;
        }
    }
}


