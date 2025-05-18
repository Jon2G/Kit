using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kit.Forms.Extensions;
using Xamarin.Forms;

namespace Kit.Forms.Controls.CrossImage
{
    public class CrossImage : Kit.Controls.CrossImage.CrossImage
    {
        public override byte[] ToArray()
        {
            if (Native is ImageSource ximage)
            {
                return Extensions.ImageExtensions.ImageToByte(ximage);
            }
            return null;
        }

        public override async Task<Stream> ToStream()
        {
            if (Native is StreamImageSource simage)
            {
                return await simage.Stream.Invoke(CancellationToken.None);
            }
            if (Native is FileImageSource fimage)
            {
                return await Task.FromResult<Stream>(fimage.ImageToStream());
            }
            if (Native is ImageSource ximage)
            {
                Stream stream = (Stream)new MemoryStream(this.ToArray());
                return await Task.FromResult(stream);
            }
            return await Task.FromResult<Stream>(null);
        }
    }
}
