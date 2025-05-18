using AsyncAwaitBestPractices.MVVM;
using Kit.Extensions;
using Serilog.Events;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
namespace Kit.Forms.Controls.LogsViewer
{
    public class LogViewerModel : IDisposable
    {
        public int MaxLogs { get; set; }
        public ObservableCollection<LogMsg> Logs { get; set; }
        private ICommand _AlertLogCommand;
        public ICommand AlertLogCommand => _AlertLogCommand ??= new AsyncCommand<LogMsg>(AlertLog);


        private Task AlertLog(LogMsg msg)
        {
#if ANDROID || IOS  || MACCATALYST
            Acr.UserDialogs.UserDialogs.Instance.AlertAsync(msg.Text, msg.Level, "Ok");
#endif
            throw new NotSupportedException("AlertLog is not supported on this platform.");
        }

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