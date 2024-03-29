﻿using System;
using System.Linq;
using System.Windows;
using Kit.Enums;
using Application = System.Windows.Application;
using Kit.WPF.Services;
using Serilog;
using System.Reflection;
using Kit.Services.Interfaces;
using Kit.Dialogs;
using Kit.Services.BarCode;

namespace Kit.WPF
{
    public class ToolsImplementation : AbstractTools
    {
        public ToolsImplementation(string LibraryPath = null)
        {
            this._LibraryPath = LibraryPath;
        }

        public string ProductName()
        {
            return Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute))
                .OfType<AssemblyProductAttribute>()
                .FirstOrDefault().Product;
        }

        private string _LibraryPath;
        public override string LibraryPath => _LibraryPath ?? Environment.CurrentDirectory;
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.WPF;
        public static new Kit.WPF.ToolsImplementation Instance => Tools.Instance as Kit.WPF.ToolsImplementation;

        public override AbstractTools Init()
        {
            Kit.Tools.Container.Register<ISynchronizeInvoke, SynchronizeInvoke>();
            Kit.Tools.Container.Register<IDialogs, Kit.WPF.Dialogs.Dialogs>();
            Kit.Tools.Container.Register<IScreenManager, ScreenManagerService>();
            Kit.Tools.Container.Register<Kit.Controls.CrossImage.CrossImageExtensions, Kit.WPF.Controls.CrossImage.CrossImageExtensions>();
            Kit.Tools.Container.Register<IBarCodeBuilder, BarCodeBuilder>();
            Kit.Tools.Container.Register<IClipboardService, ClipboardService>();
#if NET6_0_OR_GREATER
//Kit.Tools.Container.Register<Plugin.DeviceInfo.Abstractions.IDeviceInfo,>();
#else
            Kit.Tools.Container.Register<Plugin.DeviceInfo.Abstractions.IDeviceInfo, Plugin.DeviceInfo.DeviceInfoImplementation>();
#endif

            Log.Init(loggerFactory: (log) => (new LoggerConfiguration()
                    // Set default log level limit to Debug
                    .MinimumLevel.Verbose()
                    // Enrich each log entry with memory usage and thread ID
                    // .Enrich.WithMemoryUsage()
                    //.Enrich.WithThreadId()
                    // Write entries to Android log (Nuget package Serilog.Sinks.Xamarin)
                    .WriteTo.Console().MinimumLevel.Verbose()
                    .WriteTo.Debug().MinimumLevel.Verbose()
                    // Create a custom logger in order to set another limit,
                    // particularly, any logs from Information level will also be written into a rolling file
                    .WriteTo.Logger(config =>
                    config
                            .MinimumLevel.Verbose()
                            .WriteTo.File(log.LoggerPath, retainedFileCountLimit: 7,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(500))
                    )
                    // And create another logger so that logs at Fatal level will immediately send email
                    .WriteTo.Logger(config =>
                        config
                            .MinimumLevel.Fatal()
                            .WriteTo.File(log.CriticalLoggerPath, retainedFileCountLimit: 1,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(500))
                    )).CreateLogger(), CriticalAction: CriticalAlert);
            base.Init();
            return this;
        }
        public Window VentanaPadre()
        {
            if (IsInDesingMode)
            {
                return null;
            }
            try
            {
                Window a = (from nic in Application.Current.Windows.OfType<Window>()
                            where nic.IsActive //&& nic.GetType() != typeof(Chat)
                            select nic).FirstOrDefault();
                if (!(a is null)) return a.IsActive ? a : null;
                a = Application.Current.Windows.OfType<Window>().FirstOrDefault();
                if (a != null && a.IsActive) return a.IsActive ? a : null;
                try
                {
                    a?.Show();
                }
                catch (Exception ex)
                {
                    // ignored
                    Log.Logger.Error(ex, "Ventana padre");
                }

                return a.IsActive ? a : null;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al determinar la ventana padre");
                return null;
            }
        }

        public static T Abrir<T>(Window ventana, bool Modal = true)
        {
            if (ventana.Owner is null)
            {
                ventana.Owner = Instance.VentanaPadre();
            }
            ventana.WindowStartupLocation = ventana.Owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
            if (Modal)
                ventana.ShowDialog();
            else
                ventana.Show();
            return (T)(object)ventana;
        }

        public static void Abrir(Window ventana, bool Modal = true)
        {
            Abrir<Window>(ventana, Modal);
        }
    }
}