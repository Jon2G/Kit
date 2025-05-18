using Kit.Services.Interfaces;

namespace Kit.Forms.Services
{
    [Preserve(AllMembers = true)]
    public class SynchronizeInvoke : ISynchronizeInvoke
    {
        public async Task InvokeOnMainThreadAsync(Action action)
        {
            await Device.InvokeOnMainThreadAsync(action);
        }

        public void BeginInvokeOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }

        public Task<T> InvokeOnMainThreadAsync<T>(Func<T> action)
        {
            return Device.InvokeOnMainThreadAsync(action);
        }

        public T BeginInvokeOnMainThread<T>(Func<T> action)
        {
            return Device.InvokeOnMainThreadAsync(action).GetAwaiter().GetResult();
        }
    }
}
