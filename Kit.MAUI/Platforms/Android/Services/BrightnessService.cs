using Android.Views;
using Kit.Droid.Services;
using Kit.Services.Interfaces;
using Plugin.CurrentActivity;

[assembly: Dependency(typeof(BrightnessService))]
namespace Kit.Droid.Services
{
    public class BrightnessService : IBrightnessService
    {
        public void SetBrightness(float brightness)
        {
            var window = CrossCurrentActivity.Current.Activity.Window;
            WindowManagerLayoutParams attributesWindow = new WindowManagerLayoutParams();

            attributesWindow.CopyFrom(window.Attributes);
            attributesWindow.ScreenBrightness = brightness;

            window.Attributes = attributesWindow;
        }
        public float GetBrightness()
        {
            var window = CrossCurrentActivity.Current.Activity.Window;
            WindowManagerLayoutParams attributesWindow = new WindowManagerLayoutParams();

            attributesWindow.CopyFrom(window.Attributes);
            return attributesWindow.ScreenBrightness;
        }
    }
}
