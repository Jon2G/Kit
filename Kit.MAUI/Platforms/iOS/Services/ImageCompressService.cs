using FFImageLoading.Extensions;
using Foundation;
using Kit.Forms.Services.Interfaces;
using Kit.iOS.Services;
using UIKit;

[assembly: Dependency(typeof(ImageCompressService))]

namespace Kit.iOS.Services
{
    [Preserve(AllMembers = true)]
    public class ImageCompressService : IImageCompressService
    {
        public async Task<FileStream> CompressImage(Stream imageData, int Quality)
        {
            await Task.Yield();
            string path = System.IO.Path.Combine(Kit.Tools.Instance.TemporalPath, $"{Guid.NewGuid():N}.jpeg");
            using (NSData nsData = NSData.FromStream(imageData))
            {
                using (UIImage uiimage = UIImage.LoadFromData(nsData))
                {
                    using (Stream compressed = uiimage.AsJpegStream(Quality))
                    {
                        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
                        await compressed.CopyToAsync(stream);
                        return stream;
                    }
                }
            }
        }
    }
}
