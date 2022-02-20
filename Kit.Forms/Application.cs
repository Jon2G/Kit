﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Kit.Forms.Pages;
namespace Kit.Forms
{
    public abstract class Application : Xamarin.Forms.Application
    {
        protected override void OnSleep()
        {
            OnSleep(Xamarin.Forms.Application.Current.MainPage);
        }
        private void OnSleep(Page page)
        {
            switch (page)
            {
                case Shell shell:
                    OnSleep(shell.CurrentPage);
                    return;
                case BasePage basePage:
                    basePage.OnSleep();
                    break;
            }
        }
    }
}
