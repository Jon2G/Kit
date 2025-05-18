using Android.App;
using Android.Views;
using Kit.Enums;
using Kit.Services.Interfaces;

namespace Kit.Droid.Services
{
    public class ScreenManagerService : IScreenManager
    {
        private readonly Activity Activity;
        public ScreenManagerService()
        {
            Activity = (Kit.Tools.Instance as Droid.ToolsImplementation).MainActivity;
        }
        public void SetScreenMode(ScreenMode ScreenMode)
        {

            Activity.Window.ClearFlags(WindowManagerFlags.Fullscreen);

            int uiOptions = (int)Activity.Window.DecorView.SystemUiVisibility;
            uiOptions |= (int)SystemUiFlags.LowProfile;
            switch (ScreenMode)
            {
                case ScreenMode.FullScreen:
                    Activity.Window.AddFlags(WindowManagerFlags.Fullscreen);
                    uiOptions |= (int)SystemUiFlags.Fullscreen;
                    uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
                    uiOptions |= (int)SystemUiFlags.HideNavigation;
                    break;
                case ScreenMode.HideControlsBar:
                    Activity.Window.AddFlags(WindowManagerFlags.Fullscreen);
                    break;
                case ScreenMode.Normal:
                    uiOptions |= (int)SystemUiFlags.Visible;
                    break;

            }
            Activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }
    }
}
