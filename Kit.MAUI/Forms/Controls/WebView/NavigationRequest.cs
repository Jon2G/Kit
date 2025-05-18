using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kit.Forms.Controls.WebView
{
    public class NavigationRequest : IDisposable
    {
        private readonly ManualResetEvent NavigatedCallback;
        public readonly Uri Url;
        public readonly Guid RequestGuid;
        public bool IsComplete = false;
        public NavigationRequest(string Url)
        {
            this.Url = new Uri(Url);
            this.NavigatedCallback = new ManualResetEvent(false);
            this.RequestGuid = Guid.NewGuid();
        }

        public async Task Wait()
        {
            if (this.IsComplete)
            {
                await Task.CompletedTask;
                return;
            }
            this.NavigatedCallback.Reset();
            Log.Logger.Debug("WAIT - {0} - {1}", RequestGuid, Url);
            await Task.Run(() => this.NavigatedCallback.WaitOne());
            Log.Logger.Debug("RELEASE - {0} - {1}", RequestGuid, Url);
        }

        public void Done()
        {
            this.NavigatedCallback.Set();
            Log.Logger.Debug("DONE - {0} - {1}", RequestGuid, Url);
        }

        public void Dispose()
        {
            NavigatedCallback?.Dispose();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.RequestGuid).Append(" - ").Append(this.Url.AbsoluteUri);
            return sb.ToString();
        }
    }
}
