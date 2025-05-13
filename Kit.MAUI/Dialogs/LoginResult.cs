namespace Kit.Dialogs
{
    public class LoginResult
    {
        public bool Ok { get; }
        public string UserName { get; }
        public string Password { get; }

        public LoginResult(bool Ok, string UserName, string Password)
        {
            this.Ok = Ok;
            this.UserName = UserName;
            this.Password = Password;
        }
    }
}