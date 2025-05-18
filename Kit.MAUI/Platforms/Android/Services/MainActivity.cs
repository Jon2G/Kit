using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Kit.Forms.Services;
using Kit.Forms.Services.Interfaces;
using Plugin.CurrentActivity;
using System.Net;

[assembly: UsesFeature("android.hardware.camera", Required = false)]
[assembly: UsesPermission("android.permission.ACCESS_WIFI_STATE")]
namespace Kit.Droid.Services
{
    [MetaData(name: "android.support.FILE_PROVIDER_PATH", Resource = "@xml/paths")]
    [Activity(Label = "Kit.MainActivity", Icon = "@mipmap/icon", Theme = "@style/Maui.SplashTheme", MainLauncher = false, LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden |
            ConfigChanges.Mcc | ConfigChanges.Mnc | ConfigChanges.Navigation
    )]
    public abstract class MainActivity : MauiAppCompatActivity, IUpdateWidget
    {
        protected virtual void Initialize(Bundle savedInstanceState, Type CurrentApp)
        {
            Kit.Droid.Tools.Init(this, savedInstanceState);
            FontCache.DeleteFontCacheIfFontChanged(CurrentApp);
        }

        public static MainActivity Instance;

        public MainActivity()
        {

        }
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            IKeyboardListenerService.Current?.SetIsKeyboardPluggedIn(IsKeyboardPluggedIn(newConfig));
        }

        public bool IsKeyboardPluggedIn(Configuration configuration)
        {
            return (configuration ?? this.Resources.Configuration).Keyboard !=
                Android.Content.Res.KeyboardType.Nokeys;
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            try
            {
                if (!e.Flags.HasFlag(Android.Views.KeyEventFlags.SoftKeyboard) && !e.Flags.HasFlag(KeyEventFlags.VirtualHardKey))
                {
                    char character = ((char)e.GetUnicodeChar(e.MetaState));
                    IKeyboardListenerService.Current?.OnKeyUp(character);
                    if (Kit.Tools.Debugging)
                    {
                        Log.Logger.Debug("Keyboard:{0}", character);
                    }
                }
            }
            catch (Exception ex) { Log.Logger?.Error(ex, "OnKeyUp"); }
            return base.OnKeyUp(keyCode, e);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //global::Xamarin.Forms.Forms.SetFlags("Shapes_Experimental", "DragAndDrop_Experimental");
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            //global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            //CachedImageRenderer.InitImageViewHandler();
            //CachedImageRenderer.Init(false);
            UserDialogs.Init(this);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            //Rg.Plugins.Popup.Popup.Init(this);
            Kit.Droid.Tools.Init(this, savedInstanceState);
            //Forms9Patch.Droid.Settings.Initialize(this);
            //TinyIoC.TinyIoCContainer.Current.Register<IUpdateWidget>(this);
            //TinyIoC.TinyIoCContainer.Current.Register<IImageCompressService>(new ImageCompressService());
            Instance = this; //ImagePicker
            ServicePointManager.ServerCertificateValidationCallback += (o, cert, chain, errors) => true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            Serilog.Log.CloseAndFlush();
            base.OnDestroy();
        }

        public static Context GetAppContext()
        {
            return Android.App.Application.Context;
        }

        public void UpdateWidget(string AppWidgetProviderFullClassName)
        {
            using (ReflectionCaller caller = ReflectionCaller.FromThis(this))
            {
                var context = CrossCurrentActivity.Current.AppContext;
                var type = caller.GetType(AppWidgetProviderFullClassName);
                context.UpdateWidget(type);
            }
        }
    }
}