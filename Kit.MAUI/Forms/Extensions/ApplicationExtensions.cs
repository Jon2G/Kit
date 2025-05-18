
namespace Kit.Forms
{
    public static class ApplicationExtensions
    {
        public static T GetResource<T>(this Microsoft.Maui.Controls.Application app, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            return (T)app.Resources[key];
        }
        public static bool IsOnDarkTheme(this Microsoft.Maui.Controls.Application app)
        {
            return app.RequestedTheme == AppTheme.Dark;
        }
        public static bool IsOnLightTheme(this Microsoft.Maui.Controls.Application app)
        {
            return !IsOnDarkTheme(app);
        }

    }
}
