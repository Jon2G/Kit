using System.Net.NetworkInformation;

namespace Kit
{
    public static class PingExtensions
    {
        public static Task<PingReply> PingOrTimeout(string hostname, int timeOut = 250) => PingOrTimeout(new Ping(), hostname, timeOut);
        public static async Task<PingReply> PingOrTimeout(this Ping ping, string hostname, int timeOut = 250)
        {
            await Task.Yield();
            PingReply result = null;
            var cancellationTokenSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeOut, cancellationTokenSource.Token);

            var actionTask = Task.Factory.StartNew(() =>
            {
                result = ping.RegularPing(hostname, timeOut);
            }, cancellationTokenSource.Token);

            await Task.WhenAny(actionTask, timeoutTask).ContinueWith(t =>
            {
                cancellationTokenSource.Cancel();
            });
            return result;
        }

        private static PingReply RegularPing(string hostname, int timeout)
            => RegularPing(new Ping(), hostname, timeout);
        public static PingReply RegularPing(this Ping ping, string hostname, int timeout)
        {
            try
            {
                return ping.Send(hostname, timeout);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "normalPing");
                throw ex;
            }
        }
    }
}
