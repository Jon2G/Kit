using Android.Graphics;
using Kit.Droid.Services;
using Kit.Forms.Services.Interfaces;

[assembly: Dependency(typeof(ImageResizerService))]
namespace Kit.Droid.Services
{
    public class ImageResizerService : IImageResizer
    {
        public static async Task<FileStream> ResizeImageAndroid(Stream imageData, float width, float height)
        {
            await Task.Yield();
            // Load the bitmap
            byte[] bytes = await Kit.Helpers.GetByteArray(imageData);
            Bitmap originalImage = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);
            string path = System.IO.Path.Combine(Kit.Tools.Instance.TemporalPath, $"{Guid.NewGuid():N}.jpeg");
            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);

                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    ms.Position = 0;
                    ms.CopyTo(fileStream);
                    return fileStream;
                }
            }
        }

        public Task<FileStream> ResizeImage(Stream imageData, float width, float height) => ResizeImageAndroid(imageData, width, height);

    }
}
