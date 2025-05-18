using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Microsoft.Maui.Devices;
using Kit.Droid.Services;

namespace Kit.Droid
{
    public class Tools : Kit.Tools
    {
        public static AbstractTools Init(MainActivity activity, Bundle bundle)
        {
            //Xamarin.Essentials.Platform.Init(activity, bundle);
            //Xamarin.Forms.Forms.Init(activity, bundle);

            //Acr.UserDialogs.UserDialogs.Init(activity);
            CrossCurrentActivity.Current.Init(activity, bundle);
            //Rg.Plugins.Popup.Popup.Init(activity);

            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;
            Set(new ToolsImplementation());
            (Instance as ToolsImplementation).Init(activity);
            CrossCurrentActivity.Current.Init(activity, bundle);
            
            //ZXing.Net.Mobile.Forms.Android.Platform.Init();
            OrientationServices(activity);
            BaseInit();
            return Instance;
        }
   
        private static void OrientationServices(Activity activity)
        {
            MessagingCenter.Subscribe<Page>(activity, nameof(Microsoft.Maui.Devices.DisplayOrientation.Landscape), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Landscape;
            });
            MessagingCenter.Subscribe<Page>(activity, nameof(DisplayOrientation.Portrait), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Portrait;
            });
            MessagingCenter.Subscribe<Page>(activity, nameof(DisplayOrientation.Unknown), sender =>
            {
                activity.RequestedOrientation = ScreenOrientation.Unspecified;
            });
        }
    }
}
