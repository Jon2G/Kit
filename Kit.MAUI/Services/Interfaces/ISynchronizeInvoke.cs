namespace Kit.Services.Interfaces
{
    public interface ISynchronizeInvoke
    {
        Task InvokeOnMainThreadAsync(Action action);
        Task<T> InvokeOnMainThreadAsync<T>(Func<T> action);
        T BeginInvokeOnMainThread<T>(Func<T> action);
        void BeginInvokeOnMainThread(Action action);
    }
}
