using System.IO;
using System.Threading.Tasks;

namespace Kit.Forms.Services.Interfaces
{
    public interface IImageResizer
    {
        public Task<FileStream> ResizeImage(Stream imageData, float width, float height);
    }
}
