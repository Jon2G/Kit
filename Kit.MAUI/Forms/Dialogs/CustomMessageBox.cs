using Kit.Dialogs;
using Kit.Enums;
using Kit.Forms.Dialogs;

[assembly: Dependency(typeof(CustomMessageBox))]

namespace Kit.Forms.Dialogs
{
    public class CustomMessageBox : ICustomMessageBox
    {
        public async Task<CustomMessageBoxResult> Show(string messageBoxText)
        {
#if ANDROID || IOS  || MACCATALYST
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(messageBoxText);
            return CustomMessageBoxResult.OK;
#endif
            throw new NotSupportedException();
        }

        public async Task<CustomMessageBoxResult> Show(string messageBoxText, string caption)
        {
#if ANDROID || IOS  || MACCATALYST
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(messageBoxText, caption);
            return CustomMessageBoxResult.OK;
#endif
            throw new NotSupportedException();
        }

        public async Task<CustomMessageBoxResult> Show(string messageBoxText, string caption, CustomMessageBoxButton button)
        {
#if ANDROID || IOS  || MACCATALYST
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(messageBoxText, caption, "Ok");
            return CustomMessageBoxResult.OK;
#endif
            throw new NotSupportedException();
        }

        public async Task<CustomMessageBoxResult> Show(string messageBoxText, string caption, CustomMessageBoxButton button, CustomMessageBoxImage icon)
        {
#if ANDROID || IOS || MACCATALYST
            string text = null;
            switch (button)
            {
                case CustomMessageBoxButton.OK:
                    text = "Ok";
                    break;
            }
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(messageBoxText, caption, text);
            return CustomMessageBoxResult.OK;
#endif
            throw new NotSupportedException();
        }

        public async Task<CustomMessageBoxResult> ShowOK(string messageBoxText, string caption, string okButtonText)
        {
#if ANDROID || IOS  || MACCATALYST
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(messageBoxText, caption, okButtonText);
            return CustomMessageBoxResult.OK;
#endif
            throw new NotSupportedException();
        }

        public async Task<CustomMessageBoxResult> ShowOK(string messageBoxText, string caption, string okButtonText, CustomMessageBoxImage icon)
        {
#if ANDROID || IOS  || MACCATALYST
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(messageBoxText, caption, okButtonText);
            return CustomMessageBoxResult.OK;
#endif
            throw new NotSupportedException();
        }

        public async Task<CustomMessageBoxResult> ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText)
        {
            await Task.Yield();
            return CustomMessageBoxResult.None;
        }

        public async Task<CustomMessageBoxResult> ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText, CustomMessageBoxImage icon)
        {
            await Task.Yield();
            return CustomMessageBoxResult.None;
        }

        public async Task<CustomMessageBoxResult> ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText)
        {
            await Task.Yield();
            return CustomMessageBoxResult.None;
        }

        public async Task<CustomMessageBoxResult> ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText, CustomMessageBoxImage icon)
        {
            await Task.Yield();
            return CustomMessageBoxResult.None;
        }

        public async Task<CustomMessageBoxResult> ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            await Task.Yield();
            return CustomMessageBoxResult.None;
        }

        public async Task<CustomMessageBoxResult> ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, CustomMessageBoxImage icon)
        {
            await Task.Yield();
            return CustomMessageBoxResult.None;
        }
    }
}