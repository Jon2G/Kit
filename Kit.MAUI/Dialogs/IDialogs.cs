namespace Kit.Dialogs
{
    public interface IDialogs
    {
        ILoading Loading { get; }
        ICustomMessageBox CustomMessageBox { get; }

        public Task<LoginResult> LoginAsync(string title = null, string message = null);

        public Task<LoginResult> LoginAsync(LoginConfig config);
    }
}