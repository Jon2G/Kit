using Kit.Forms.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Forms.Controls.CrossImage
{
    public class CrossImageExtensions : Kit.Controls.CrossImage.CrossImageExtensions
    {
        public override Kit.Controls.CrossImage.CrossImage FromFile(FileInfo fileInfo)
        {
            Kit.Forms.Controls.CrossImage.CrossImage image = new Kit.Forms.Controls.CrossImage.CrossImage();
            image.Native = ImageSource.FromFile(fileInfo.FullName);
            return image;
        }

        public override Kit.Controls.CrossImage.CrossImage FromStream(Func<Stream> stream)
        {
            Kit.Forms.Controls.CrossImage.CrossImage image = new Kit.Forms.Controls.CrossImage.CrossImage();
            image.Native = ImageSource.FromStream(stream);
            return image;
        }

        public override Kit.Controls.CrossImage.CrossImage FromByteArray(byte[] imagen)
        {
            Kit.Forms.Controls.CrossImage.CrossImage image = new Kit.Forms.Controls.CrossImage.CrossImage();
            image.Native = ImageSource.FromStream(() => new MemoryStream(imagen));
            return image;
        }

        public override Task<byte[]> GetByteArray(Kit.Controls.CrossImage.CrossImage CrossImage)
        {
            StreamImageSource streamImageSource = (StreamImageSource)CrossImage.Native;
            return Kit.Forms.Extensions.ImageExtensions.GetByteArray(streamImageSource);
        }

        public static byte[] ImageToByte(CrossImage ImageSource)
        {
            StreamImageSource streamImageSource = (StreamImageSource)ImageSource.Native;
            return streamImageSource.ImageToByte();
        }

        public static Stream ImageToStream(CrossImage CrossImage)
        {
            ImageSource ImageSource = (ImageSource)CrossImage.Native;
            return ImageSource.ImageToStream();
        }
    }
}