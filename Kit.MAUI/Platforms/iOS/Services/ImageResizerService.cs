using CoreGraphics;
using Kit.iOS.Services;
using System.Drawing;
using UIKit;

[assembly: Dependency(typeof(ImageResizerService))]
namespace Kit.iOS.Services
{
    public class ImageResizerService : Kit.Forms.Services.Interfaces.IImageResizer
    {
        public static UIKit.UIImage ImageFromByteArray(Stream data)
        {
            if (data == null)
            {
                return null;
            }

            UIKit.UIImage image;
            try
            {
                image = new UIKit.UIImage(Foundation.NSData.FromStream(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }
        public static FileStream ResizeImageIOS(Stream imageData, float width, float height)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            UIImageOrientation orientation = originalImage.Orientation;

            //create a 24bit RGB image
            using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
                (int)width, (int)height, 8,
                4 * (int)width, CGColorSpace.CreateDeviceRGB(),
                CGImageAlphaInfo.PremultipliedFirst))
            {

                RectangleF imageRect = new RectangleF(0, 0, width, height);

                // draw the image
                context.DrawImage(imageRect, originalImage.CGImage);

                UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage(), 0, orientation);
                string path = Path.Combine(Kit.Tools.Instance.TemporalPath, $"{Guid.NewGuid():N}.jpeg");
                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (MemoryStream memoryStream = new MemoryStream(resizedImage.AsJPEG().ToArray()))
                    {
                        memoryStream.Position = 0;
                        memoryStream.CopyTo(fileStream);
                        return fileStream;
                    }
                }
                // save the image as a jpeg
            }
        }

        public Task<FileStream> ResizeImage(Stream imageData, float width, float height)
        {
            return Task.FromResult(ResizeImageIOS(imageData, width, height));
        }
    }
}