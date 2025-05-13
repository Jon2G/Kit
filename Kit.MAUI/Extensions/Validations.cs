using System.Text.RegularExpressions;

namespace Kit
{
    public static class Validations
    {
        private static Regex EmailRegex => _EmailRegex.Value;
        private static readonly Lazy<Regex> _EmailRegex = new Lazy<Regex>(() => new Regex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.Compiled | RegexOptions.Singleline));
        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && EmailRegex.IsMatch(email);
        }
    }
}
