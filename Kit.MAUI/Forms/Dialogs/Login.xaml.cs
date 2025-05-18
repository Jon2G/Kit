using AsyncAwaitBestPractices;
using Kit.Dialogs;
using System;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Dialogs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login
    {
        public LoginConfig Config { get; }
        public bool Ok { get; private set; }

        public Login(LoginConfig config)
        {
            Config = config;
            this.BindingContext = config;
            InitializeComponent();
            this.LockModal();
        }

        private void OnOk(object sender, EventArgs e)
        {
            Ok = true;
            this.Close().SafeFireAndForget();
        }

        private void Cancel(object sender, EventArgs e)
        {
            Ok = false;
            this.Close().SafeFireAndForget();
        }
    }
}