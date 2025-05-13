using Serilog.Core;
using Serilog.Events;
using System.Windows.Input;

namespace Kit.Extensions
{
    public struct LogMsg
    {
        public LogEventLevel EventLevel { get; private set; }
        public string Level { get; private set; }
        public string Text { get; private set; }
        public string TimeStamp { get; private set; }

        public LogMsg(LogEventLevel EventLevel, string Level, string Text, string TimeStamp)
        {
            this.EventLevel = EventLevel;
            this.Level = Level;
            this.Text = Text;
            this.TimeStamp = TimeStamp;
        }
    }

    public class LogsSink : ILogEventSink
    {
        public ICommand OnLogEmit { get; set; }

        public LogsSink()
        {
        }

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write</param>
        public void Emit(LogEvent logEvent)
        {
            string Text = logEvent.RenderMessage();
            string Lvl = LevelToSeverity(logEvent);
            string TimeStamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " -" + logEvent.Timestamp.Offset.ToString(@"hh\:mm");
            OnLogEmit?.Execute(new LogMsg(logEvent.Level, Lvl, Text, TimeStamp));
        }

        private static string LevelToSeverity(LogEvent logEvent)
        {
            switch (logEvent.Level)
            {
                case LogEventLevel.Debug:
                    return "[DBG]";

                case LogEventLevel.Error:
                    //MainPageViewModel.statusBar.AddError();
                    return "[ERR]";

                case LogEventLevel.Fatal:
                    return "[FTL]";

                case LogEventLevel.Verbose:
                    return "VERBOSE";

                case LogEventLevel.Warning:
                    //MainPageViewModel.statusBar.AddWarning();
                    return "WARNING";

                default:
                    return "[INF]";
            }
        }
    }
}