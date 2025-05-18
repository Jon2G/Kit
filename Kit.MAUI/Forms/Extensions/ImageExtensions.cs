using System.IO;
using System.Threading.Tasks;
using Kit.Forms.Services.Interfaces;
using Xamarin.Forms;

namespace Kit.Forms.Extensions
{
    public static partial class ImageExtensions
    {
        public static async Task<FileImageSource> ResizeImage(this FileImageSource imageData, float width, float height)
        {
            await Task.Yield();
            FileStream file = await ResizeImage(imageData.ImageToStream(), width, height);
            return (FileImageSource)FileImageSource.FromFile(file.Name);
        }
        public static Task<FileStream> ResizeImage(Stream imageData, float width, float height) => DependencyService.Get<IImageResizer>().ResizeImage(imageData, width, height);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="Quality">From 0 to 100</param>
        /// <returns></returns>
        public static async Task<FileImageSource> CompressImage(this FileImageSource imageData, int Quality)
        {
            await Task.Yield();
            FileStream file = await CompressImage(imageData.ImageToStream(), Quality);
            return (FileImageSource)FileImageSource.FromFile(file.Name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="Quality">From 0 to 100</param>
        /// <returns></returns>
        public static Task<FileStream> CompressImage(Stream imageData, int Quality)
        {
            var service = TinyIoC.TinyIoCContainer.Current.Resolve<IImageCompressService>();
            return service.CompressImage(imageData, Quality);
        }
        public static ImageSource ByteToImage(this byte[] ByteArray)
        {
            return ImageSource.FromStream(() => new MemoryStream(ByteArray));
        }

        public static async Task<byte[]> GetByteArray(StreamImageSource streamImageSource)
        {
            Stream stream = await streamImageSource.Stream.Invoke(System.Threading.CancellationToken.None);
            return await stream.GetByteArray();
        }

        public static byte[] ImageToByte(this ImageSource ImageSource)
        {
            StreamImageSource streamImageSource = (StreamImageSource)ImageSource;
            System.Threading.CancellationToken cancellationToken = System.Threading.CancellationToken.None;
            Task<Stream> task = streamImageSource.Stream(cancellationToken);
            MemoryStream stream = task.Result as MemoryStream;
            return stream.ToArray();
        }

        public static Stream ImageToStream(this ImageSource ImageSource)
        {
            Stream stream = null;
            switch (ImageSource)
            {
                case StreamImageSource streamImageSource:
                    System.Threading.CancellationToken cancellationToken = System.Threading.CancellationToken.None;
                    Task<Stream> task = streamImageSource.Stream(cancellationToken);
                    stream = task.Result as MemoryStream;
                    break;

                case FileImageSource fileImageSource:
                    stream = new FileStream(fileImageSource.File, FileMode.Open, FileAccess.Read);
                    break;
            }
            return stream;
        }
    }
}