

namespace Kit.Forms.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OtgCodeBarScannerDetectorPage : ContentPage
    {
        public OtgCodeBarScannerDetectorPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.Model.Init(BeginView, CenterView, EndView);
        }
    }
}