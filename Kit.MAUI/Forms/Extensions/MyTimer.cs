using System;
using System.Threading;
using Xamarin.Forms;

namespace Kit.Forms.Extensions
{
    public class MyTimer
    {
        private readonly TimeSpan timespan;
        private readonly Action callback;

        private CancellationTokenSource cancellation;
        //private TimeSpan timeSpan;
        //private Action<object> confirm;

        public MyTimer(TimeSpan timespan, Action callback)
        {
            this.timespan = timespan;
            this.callback = callback;
            this.cancellation = new CancellationTokenSource();
        }
        public MyTimer Start()
        {
            CancellationTokenSource cts = this.cancellation; // safe copy

            Device.StartTimer(this.timespan,
                () =>
                {
                    if (cts.IsCancellationRequested) return false;
                    this.callback.Invoke();
                    return false; // or true for periodic behavior
                });
            return this;
        }

        public MyTimer Stop()
        {
            Interlocked.Exchange(ref this.cancellation, new CancellationTokenSource()).Cancel();
            return this;
        }

        public void Restart()
        {
            Stop().Start();
        }
    }
}
