using Android.Graphics;
using Plugin.CurrentActivity;
using System.IO;
using System.Threading.Tasks;
using Kit.Droid.Services;
using Kit.Services.Interfaces;
using Bitmap = Android.Graphics.Bitmap;
using View = Android.Views.View;

[assembly: Xamarin.Forms.Dependency(typeof(Screenshot))]
namespace Kit.Droid.Services
{
    public class Screenshot : IScreenshot
    {
        public async Task<byte[]> Capture()
        {
            await Task.Yield();

            View rootView = CrossCurrentActivity.Current.Activity.Window.DecorView.RootView;
            using (Bitmap screenshot = Android.Graphics.Bitmap.CreateBitmap(
                rootView.Width,
                rootView.Height,
                Android.Graphics.Bitmap.Config.Argb8888))
            {
                Canvas canvas = new (screenshot);
                rootView.Draw(canvas);

                using (MemoryStream stream = new ())
                {
                    screenshot.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 90, stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
