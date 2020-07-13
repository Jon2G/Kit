﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Plugin.Xamarin.Tools.Shared;
using SQLHelper;
using Xamarin.Forms;


namespace Plugin.Xamarin.Tools.UWP
{
    /// <summary>
    /// Interface for Xamarin.Tools
    /// </summary>
    public partial class Tools
    {
        /// <summary>
        /// Initialize android user dialogs
        /// </summary>
        public static AbstractTools Init()
        {

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;

            Plugin.Xamarin.Tools.Shared.Tools.Set(new ToolsImplementation());
            Plugin.Xamarin.Tools.Shared.Tools.Instance.SetDebugging(Debugger.IsAttached);
    
            // ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init();

            #region DependencyServices
            //DependencyService.Register<DataShare>();
            //DependencyService.Register<PDFSaveAndOpen>();
            //DependencyService.Register<PhotoPickerService>();
            DependencyService.Register<UWP.Services.PrintHTML>();
            DependencyService.Register<UWP.Services.DeviceInfo>();
            // DependencyService.Register<Screenshot>();
            DependencyService.Register<UWP.Services.ICustomMessageBox.CustomMessageBoxService>();
            #endregion

            return Plugin.Xamarin.Tools.Shared.Tools.Instance;
        }

        //static AbstractTools currentInstance;
        //public static AbstractTools Instance
        //{
        //    get
        //    {
        //        if (currentInstance == null)
        //            throw new ArgumentException("[Shared.Tools] In android, you must call UserDialogs.Init(Activity) from your first activity OR UserDialogs.Init(App) from your custom application OR provide a factory function to get the current top activity via UserDialogs.Init(() => supply top activity)");

        //        return currentInstance;
        //    }
        //    set => currentInstance = value;
        //}
    }
}
