using Kit.Enums;
using Kit.Services.Interfaces;

namespace Kit.iOS.Services
{
    public class ScreenManagerService : IScreenManager
    {

        public ScreenManagerService()
        {

        }
        public void SetScreenMode(ScreenMode ScreenMode)
        {
            throw new NotImplementedException("SetScreenMode is not implemented for iOS.");
            //if (Application.Current.MainPage is NavigationPage page)
            //{
            //    switch (ScreenMode)
            //    {
            //        case ScreenMode.Normal:
            //            NavigationPage.SetHasNavigationBar(page, true);
            //            PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(page, false);
            //            break;
            //        default:
            //            NavigationPage.SetHasNavigationBar(page, false);
            //            PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(page, true);
            //            break;
            //    }
            //}

        }
    }
}
