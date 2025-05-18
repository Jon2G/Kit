using Kit.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Kit.Forms.Services
{
    [Preserve(AllMembers = true)]
    public class SynchronizeInvoke : ISynchronizeInvoke
    {
        public async Task InvokeOnMainThreadAsync(Action action)
        {
            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(action);
        }

        public void BeginInvokeOnMainThread(Action action)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(action);
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> action)
        {
            return Xamarin.Forms.Device.InvokeOnMainThreadAsync(action);
        }

        public T BeginInvokeOnMainThread<T>(Func<T> action)
        {
            return Xamarin.Forms.Device.InvokeOnMainThreadAsync(action).GetAwaiter().GetResult();
        }
    }
}
