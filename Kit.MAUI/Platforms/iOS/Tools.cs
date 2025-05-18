
using Foundation;
using Kit.iOS.Services;
using TinyTypeContainer;
using UIKit;
using UserNotifications;

namespace Kit.iOS
{
    [Preserve()]
    public class Tools : Kit.Tools
    {
        public static AbstractTools Init()
        {
            //////////////////////////////////////////
            //Xamarin.Forms.Forms.Init();
            //Rg.Plugins.Popup.Popup.Init();
            //CachedImageRenderer.Init();
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += Log.TaskSchedulerOnUnobservedTaskException;
            Set(new ToolsImplementation());
            (Instance as ToolsImplementation).Init();
            //ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
            {
                Container.Register(typeof(Kit.Forms.Services.Interfaces.IAppTrackingTransparencyPermission), new AppTrackingTransparencyPermission());
            }
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                        UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                        (approved, error) => { });
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                UIUserNotificationSettings settings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            BaseInit();
            return Kit.Tools.Instance;
        }
    }
}