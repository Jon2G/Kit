using Android.Content;
using Android.Runtime;
using Android.Views.InputMethods;
using Kit.Droid.Services;
using Kit.Droid.Utils;
using Kit.Forms.Services.Interfaces;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(KeyboardService))]
namespace Kit.Droid.Services
{
    [Preserve(AllMembers = true)]
    public class KeyboardService : Forms9Patch.Droid.KeyboardService, IKeyboardService, KeyboardUtils.SoftKeyboardToggleListener
    {
        public bool IsVisible { get; private set; }
        public KeyboardService()
        {
            KeyboardUtils.addKeyboardToggleListener(CrossCurrentActivity.Current.Activity, this);
        }
        public void onToggleSoftKeyboard(bool isVisible)
        {
            this.IsVisible = isVisible;
            Log.Logger.Debug("keyboard visible: {0}", this.IsVisible);
        }

        public void Show()
        {
            if (!IsVisible)
            {
                Toggle();
            }
        }
        public void Toggle()
        {
            InputMethodManager inputMethodManager =
                (InputMethodManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.InputMethodService);
            inputMethodManager?.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }

        public void Close()
        {
            InputMethodManager inputMethodManager =
                (InputMethodManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.InputMethodService);
            inputMethodManager?.ToggleSoftInput(ShowFlags.Implicit, 0);
        }
    }
}
