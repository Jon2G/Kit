using AsyncAwaitBestPractices.MVVM;
using Kit.Extensions;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Serilog.Events;
namespace Kit.Forms.Controls.LogsViewer
{
    public class LogViewerModel : IDisposable
    {
        public int MaxLogs { get; set; }
        public ObservableCollection<LogMsg> Logs { get; set; }
        private ICommand _AlertLogCommand;
        public ICommand AlertLogCommand => _AlertLogCommand ??= new AsyncCommand<LogMsg>(AlertLog);

        private Task AlertLog(LogMsg msg) => Acr.UserDialogs.UserDialogs.Instance.AlertAsync(msg.Text, msg.Level, "Ok");

        private readonly object _syncLock = new object();

        public LogViewerModel()
        {
            Logs = new ObservableCollection<LogMsg>();
            Log.LogsSink.OnLogEmit = new AsyncCommand<LogMsg>(OnLogEmit);
            MaxLogs = 500;
            BindingBase.EnableCollectionSynchronization(Logs, _syncLock, ObservableCollectionCallback);
        }

        private void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            // `lock` ensures that only one thread access the collection at a time
            lock (collection)
            {
                accessMethod?.Invoke();
            }
        }

        private async Task OnLogEmit(LogMsg msg)
        {
            await Task.Delay(100);
            if (msg.EventLevel == LogEventLevel.Debug)
            {
                return;
            }
            await Device.InvokeOnMainThreadAsync(() =>
             {
                 if (Logs.Count > MaxLogs)
                 {
                     Logs.Clear();
                 }
                 Logs.Insert(0, msg);
             });
        }

        public void Dispose()
        {
            Log.LogsSink.OnLogEmit = null;
        }
    }
}