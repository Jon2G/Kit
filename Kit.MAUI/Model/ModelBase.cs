using System.Runtime.CompilerServices;

namespace Kit.Model
{
    public abstract class ModelBase : INotifyPropertyChangedModel, IDisposable
    {
        protected async Task RunTask(Task task,
            bool handleException = true,
            CancellationTokenSource token = default(CancellationTokenSource), [CallerMemberName] string caller = "")
        {
            if (token != null && token.IsCancellationRequested)
                return;

            Exception exception = null;

            try
            {
                await task;
            }
            catch (TaskCanceledException)
            {
                Log.Logger.Information($"{caller} Task Cancelled");
            }
            catch (AggregateException e)
            {
                var ex = e.InnerException;
                while (ex.InnerException != null)
                    ex = ex.InnerException;

                exception = ex;
            }
            catch (Exception ex)
            {
                exception = ex;
                Log.Logger.Error($"{caller}  Error", exception);
            }
            finally
            {
                if (exception is not null && !handleException)
                {
                    throw exception;
                }
            }
        }

        protected void RunTask(Action task,
    bool handleException = true,
    CancellationTokenSource token = default(CancellationTokenSource), [CallerMemberName] string caller = "")
        {
            if (token != null && token.IsCancellationRequested)
                return;

            Exception exception = null;

            try
            {
                task.Invoke();
            }
            catch (TaskCanceledException)
            {
                Log.Logger.Information($"{caller} Task Cancelled");
            }
            catch (AggregateException e)
            {
                var ex = e.InnerException;
                while (ex.InnerException != null)
                    ex = ex.InnerException;

                exception = ex;
            }
            catch (Exception ex)
            {
                exception = ex;
                Log.Logger.Error($"{caller}  Error", exception);
            }
            finally
            {
                if (!handleException && exception is not null)
                {
                    throw exception;
                }
            }
        }

        public virtual void Dispose()
        {
        }
    }
}