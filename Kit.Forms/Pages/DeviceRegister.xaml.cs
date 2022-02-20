﻿using System;
using Kit.Dialogs;
using Kit.License;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceRegister
    {
        public DeviceRegisterModel Model { get; private set; }

        public DeviceRegister(BlumAPI.License Licence)
        {
            this.Model = new DeviceRegisterModel(Licence, this);
            this.BindingContext = this.Model;
            InitializeComponent();
            this.LockModal();
        }

        private void MailChanged(object sender, EventArgs e)
        {
            if (Btn is null)
            {
                return;
            }
            if (string.IsNullOrEmpty(this.Model.UserName) || string.IsNullOrEmpty(this.Model.Password))
            {
                this.Btn.TextColor = Color.Gray;
            }
            else
            {
                this.Btn.TextColor = Color.White;
            }
        }

        private void PasswordChanged(object sender, EventArgs e)
        {
            MailChanged(sender, e);
        }

        private void Registrarse(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri(BlumAPI.License.LoginSite));
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private async void LogIn(object sender, EventArgs e)
        {
            if (await this.Model.LogIn())
            {
                await this.Close();
            }
        }
    }
}