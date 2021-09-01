﻿using System.Threading.Tasks;
using Kit.WPF.Pages;
using BaseLicense = BlumAPI.License;

namespace Kit.WPF.Services
{
    public class License : BaseLicense
    {
        public License(string AppName) : base(AppName)
        {
        }

        protected override Task OpenRegisterForm()
        {
            DeviceRegister register = new DeviceRegister(this, new Dialogs.Dialogs())
            {
                Owner = null,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };
            register.ShowDialog();
            return Task.FromResult(true);
        }
    }
}