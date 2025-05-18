using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Controls.LogsViewer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogViewer : ContentView
    {
        public LogViewer()
        {
            InitializeComponent();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent is null)
            {
                Model?.Dispose();
            }
        }
    }
}