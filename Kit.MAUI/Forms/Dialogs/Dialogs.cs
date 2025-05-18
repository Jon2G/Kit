using Kit.Dialogs;
using Kit.Forms.Dialogs;

[assembly: Dependency(typeof(Dialogs))]

namespace Kit.Forms.Dialogs
{
    [Preserve(AllMembers = true)]
    public class Dialogs : IDialogs
    {
        public ILoading Loading => new Loading();

        private ICustomMessageBox _CustomMessageBox;
        public ICustomMessageBox CustomMessageBox => _CustomMessageBox ??= new CustomMessageBox();

        public Task<LoginResult> LoginAsync(string title = null, string message = null)
            => LoginAsync(new LoginConfig() { Title = title, Message = message });

        public async Task<LoginResult> LoginAsync(LoginConfig config)
        {
            if (string.IsNullOrEmpty(config.TitleBackground))
            {
                config.TitleBackground = KnownColor.Accent.ToHex();
            }
            Login login = new Login(config);
            await login.ShowDialog();
            LoginResult result = new LoginResult(login.Ok, config.User, config.Password);
            config.OnAction?.Invoke(result);
            return result;
        }
    }
}