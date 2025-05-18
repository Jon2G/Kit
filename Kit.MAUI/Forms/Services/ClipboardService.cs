using Kit.Forms.Services;
using Kit.Services.Interfaces;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(ClipboardService))]
namespace Kit.Forms.Services
{
    public class ClipboardService : IClipboardService
    {
        public Task<string> GetText()
        {
            return Clipboard.GetTextAsync();
        }

        public Task SetText(string text)
        {
            return Clipboard.SetTextAsync(text);
        }
    }
}
