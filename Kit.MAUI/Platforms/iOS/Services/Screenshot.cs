using Foundation;
using System.Runtime.InteropServices;
using UIKit;
using Screenshot = Kit.iOS.Services.Screenshot;
[assembly: Dependency(typeof(Screenshot))]
namespace Kit.iOS.Services
{
    public class Screenshot : Kit.Services.Interfaces.IScreenshot
    {
        public async Task<byte[]> Capture()
        {
            await Task.Yield();
            UIImage capture = UIScreen.MainScreen.Capture();
            using (NSData data = capture.AsPNG())
            {
                byte[] bytes = new byte[data.Length];
                Marshal.Copy(data.Bytes, bytes, 0, Convert.ToInt32(data.Length));
                return bytes;
            }
        }
    }
}
