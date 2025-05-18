using System.IO;
using System.Threading.Tasks;

namespace Kit.Forms.Services.Interfaces
{
    public interface IImageCompressService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="Quality">From 0 to 100</param>
        /// <returns></returns>
        public Task<FileStream> CompressImage(Stream imageData, int Quality);
    }
}
