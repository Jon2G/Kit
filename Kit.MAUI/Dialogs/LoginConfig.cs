namespace Kit.Dialogs
{
    public class LoginConfig
    {
        public string PasswordPlaceholder { get; set; } = "Password";
        public string UserPlaceholder { get; set; } = "User";
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string CancelText { get; set; } = "Cancel";
        public string OkText { get; set; } = "Ok";
        public string Message { get; set; } = "";
        public string Title { get; set; } = "Login";

        public string TitleBackground { get; set; }
        public Action<LoginResult> OnAction { get; set; }

        public LoginConfig()
        {
        }

        public LoginConfig SetAction(Action<LoginResult> action)
        {
            OnAction = action;
            return this;
        }

        public LoginConfig SetCancelText(string cancel)
        {
            CancelText = cancel;
            return this;
        }

        public LoginConfig SetMessage(string msg)
        {
            Message = msg;
            return this;
        }

        public LoginConfig SetOkText(string ok)
        {
            OkText = ok;
            return this;
        }

        public LoginConfig SetPasswordPlaceholder(string txt)
        {
            PasswordPlaceholder = txt;
            return this;
        }

        public LoginConfig SetTitle(string title)
        {
            Title = title;
            return this;
        }
    }
}