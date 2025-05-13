using System.Text.RegularExpressions;

namespace Kit
{
    public static class RegexExtensions
    {
        public static Group TryGetGroup(this Match match, string groupName)
        {
            Group group = null;
            try
            {
                if (match.Success)
                {
                    group = match.Groups[groupName];
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Group not found in match");
            }
            return group;
        }
    }
}