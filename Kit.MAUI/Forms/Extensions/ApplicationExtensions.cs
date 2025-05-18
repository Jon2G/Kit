using Xamarin.Forms;
// ReSharper disable once CheckNamespace
namespace Kit.Forms
{
    public static class ApplicationExtensions
    {
        public static T GetResource<T>(this Xamarin.Forms.Application app, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            return (T)app.Resources[key];
        }
        public static bool IsOnDarkTheme(this Xamarin.Forms.Application app)
        {
            return app.RequestedTheme == OSAppTheme.Dark;
        }
        public static bool IsOnLightTheme(this Xamarin.Forms.Application app)
        {
            return !IsOnDarkTheme(app);
        }

    }
}
