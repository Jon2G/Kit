using Foundation;
using Kit.Dialogs;
using Kit.Enums;
using Kit.Forms.Services;
using Kit.iOS.Services;
using Kit.Services.BarCode;
using Kit.Services.Interfaces;
using Serilog;
using TinyTypeContainer;
using UIKit;
namespace Kit.iOS
{
    public class ToolsImplementation : AbstractTools
    {
        public override string TemporalPath => FileSystem.CacheDirectory;
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.iOS;
        public override AbstractTools Init()
        {
            Container.Register<ISynchronizeInvoke>(new SynchronizeInvoke());
            Container.Register<IDialogs>(new Kit.Forms.Dialogs.Dialogs());
            Container.Register<IScreenManager>(new ScreenManagerService());
            Container.Register<Kit.Controls.CrossImage.CrossImageExtensions>(new Kit.Forms.Controls.CrossImage.CrossImageExtensions());
            Container.Register<IBarCodeBuilder>(new BarCodeBuilder());
            Container.Register<IClipboardService>(new ClipboardService());
            Log.Init((l) =>
            {
                return (new LoggerConfiguration()
                     // Set default log level limit to Debug
                     .MinimumLevel.Debug()
                     // Enrich each log entry with memory usage and thread ID
                     // .Enrich.WithMemoryUsage()
                     //.Enrich.WithThreadId()
                     // Write entries to ios log (Nuget package Serilog.Sinks.Xamarin)
                     //.WriteTo.NSLog()
                     // Create a custom logger in order to set another limit,
                     // particularly, any logs from Information level will also be written into a rolling file

                     .WriteTo.Async(x => x.Sink(Kit.Log.LogsSink))
                     .WriteTo.Logger(config =>
                         config
                             .MinimumLevel.Information()
                             .WriteTo.File(Log.Current.LoggerPath, retainedFileCountLimit: 7)
                     )
                     // And create another logger so that logs at Fatal level will immediately send email
                     .WriteTo.Logger(config =>
                         config
                             .MinimumLevel.Fatal()
                             .WriteTo.File(Log.Current.CriticalLoggerPath, retainedFileCountLimit: 1)
                     )).CreateLogger();
            });
            return this;
        }

        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(Page mainPage)
        {
            if (mainPage.Navigation.NavigationStack.Any() && mainPage.Navigation.NavigationStack.Last() is Page page)
            {
                if (page.GetType().GetProperty("LockedOrientation") is System.Reflection.PropertyInfo LockedOrientationProperty)
                {
                    if (LockedOrientationProperty.GetValue(page) is DisplayOrientation LockedOrientation)
                    {
                        if (LockedOrientation != DisplayOrientation.Unknown)
                        {
                            page.Disappearing += Page_Disappearing;
                            switch (LockedOrientation)
                            {
                                case DisplayOrientation.Landscape:
                                    return UIInterfaceOrientationMask.Landscape;
                                case DisplayOrientation.Portrait:
                                    return UIInterfaceOrientationMask.Portrait;

                            }
                        }
                    }
                }
            }
            return UIInterfaceOrientationMask.All;
        }
        private void Page_Disappearing(object sender, EventArgs e)
        {
            (sender as Page).Disappearing -= Page_Disappearing;
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)UIInterfaceOrientation.Unknown), new NSString("orientation"));
        }
    }
}
