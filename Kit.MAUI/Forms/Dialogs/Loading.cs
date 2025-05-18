using Kit.Dialogs;

namespace Kit.Forms.Dialogs
{
    internal class Loading : ILoading
    {
        public Loading()
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IDisposable Show(string Text = "Cargando...")
        {
#if ANDROID || IOS  || MACCATALYST
            return Acr.UserDialogs.UserDialogs.Instance.Loading(Text);
#endif
            throw new NotSupportedException();
        }
    }
}