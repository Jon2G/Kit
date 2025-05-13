using Foundation;
using UIKit;

namespace Kit.MAUI.Services
{
    public class NativeSettingsService : INativeSettingsService
    {
        public void OpenAppSettings()
        {
            string appBundleId = AppInfo.PackageName;
            var url = new NSUrl($"app-settings:{appBundleId}");

            if (UIApplication.SharedApplication.CanOpenUrl(url))
            {
                UIApplication.SharedApplication.OpenUrl(url, new UIApplicationOpenUrlOptions(), (success) =>
                {
                    if (!success)
                    {
                        Console.WriteLine("Failed to open app settings.");
                    }
                });
            }
            else
            {
                Console.WriteLine("Cannot open app settings URL.");
            }
        }
    }
}
