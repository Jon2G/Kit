using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;
using Kit.Droid.Services;
using Kit.Forms.Services.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(GalleryService))]
namespace Kit.Droid.Services
{
    public class GalleryService : IGalleryService
    {
        public async Task<string> SaveImageToGallery(Stream stream, string ImageName, string AppName = null)
        {
            AppName ??= Kit.Droid.ToolsImplementation.Instance.MainActivity.ApplicationInfo.Name;
            var resolver = Kit.Droid.ToolsImplementation.Instance.MainActivity.ContentResolver;
            var contentValues = new ContentValues();
            contentValues.Put(MediaStore.MediaColumns.DisplayName, ImageName);
            contentValues.Put(MediaStore.MediaColumns.MimeType, "image/png");
            contentValues.Put(MediaStore.MediaColumns.RelativePath, $"DCIM/{AppName}");
            var uri = resolver.Insert(MediaStore.Images.Media.ExternalContentUri, contentValues);
            string path = uri.Path;
            using (Stream openOutputStream = resolver.OpenOutputStream(uri))
            {
                await stream.CopyToAsync(openOutputStream);
            }
            return path;
        }
    }
}
