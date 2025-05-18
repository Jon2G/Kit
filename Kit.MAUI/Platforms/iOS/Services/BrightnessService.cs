using Kit.iOS.Services;
using Kit.Services.Interfaces;
using UIKit;

[assembly: Dependency(typeof(BrightnessService))]
namespace Kit.iOS.Services
{
    public class BrightnessService : IBrightnessService
    {
        public float GetBrightness()
        {
            return (float)UIScreen.MainScreen.Brightness;
        }

        public void SetBrightness(float factor)
        {
            UIScreen.MainScreen.Brightness = factor;

        }
    }
}