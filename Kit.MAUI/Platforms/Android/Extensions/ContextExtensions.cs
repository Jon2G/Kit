using System;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Java.Lang;

// ReSharper disable once CheckNamespace
namespace Kit.Droid
{
    public static class ContextExtensions
    {
        public static void UpdateWidget(this Context context,Type MyAppWidgetProvider)
        {
            Java.Lang.Class WidgetProviderType = Java.Lang.Class.FromType(MyAppWidgetProvider);
            int[] ids = AppWidgetManager.GetInstance(context)
                .GetAppWidgetIds(new ComponentName(context, WidgetProviderType));
            if (ids?.Length > 0)
            {
                Intent intent = new Intent(context, WidgetProviderType);
                intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
                // Use an array and EXTRA_APPWIDGET_IDS instead of AppWidgetManager.EXTRA_APPWIDGET_ID,
                // since it seems the onUpdate() is only fired on that:
                intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, ids);
                context.SendBroadcast(intent);
            }
        }
        public static bool IsDarkMode(this Context context)
        {
            var nightModeFlags = context.Resources?.Configuration?.UiMode ?? Android.Content.Res.UiMode.TypeNormal
                & Android.Content.Res.UiMode.NightMask;
           return nightModeFlags.HasFlag(Android.Content.Res.UiMode.NightYes);
        }
        public static bool IsServiceRunning(this Context context, Type serviceClass)
        {
            var service_class = Java.Lang.Class.FromType(serviceClass);
            ActivityManager manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
#pragma warning disable CS0618 // Type or member is obsolete
            foreach (var service in manager.GetRunningServices(Integer.MaxValue))
#pragma warning restore CS0618 // Type or member is obsolete
            {
                if (service_class.Name == service.Service.ClassName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
