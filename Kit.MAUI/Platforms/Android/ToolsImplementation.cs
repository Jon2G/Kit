using Kit.Droid.Services;
using Kit.Enums;
using Kit.Forms.Services;
using Serilog;
using TinyTypeContainer;

namespace Kit.Droid
{
    public class ToolsImplementation : AbstractTools
    {
        public override string TemporalPath => FileSystem.Current.CacheDirectory;
        public override RuntimePlatform RuntimePlatform => RuntimePlatform.Android;
        public MainActivity MainActivity { get; private set; }

        public new static ToolsImplementation Instance => Kit.Tools.Instance as ToolsImplementation;

        public void Init(MainActivity MainActivity)
        {
            this.MainActivity = MainActivity;
            Init();
        }

        public override AbstractTools Init()
        {
            Container.Register(new SynchronizeInvoke());
            Container.Register(new Kit.Forms.Dialogs.Dialogs());
            Container.Register<ScreenManagerService>(new());
            Container.Register<Kit.Forms.Controls.CrossImage.CrossImageExtensions>(new());
            Container.Register<BarCodeBuilder>(new());
            Container.Register<ClipboardService>(new());
            Container.Register<Plugin.DeviceInfo.DeviceInfoImplementation>(new());
            Log.Init((log) =>
            {
                return (new LoggerConfiguration()
                    // Set default log level limit to Debug
                    .MinimumLevel.Debug()
                    //Log to my sink logger
                    .WriteTo.Async(x => x.Sink(Log.LogsSink))
                    // Enrich each log entry with memory usage and thread ID
                    // .Enrich.WithMemoryUsage()
                    //.Enrich.WithThreadId()
                    // Write entries to Android log (Nuget package Serilog.Sinks.Xamarin)
                    //.WriteTo.Async(x => x.AndroidLog())
                    // Create a custom logger in order to set another limit,
                    // particularly, any logs from Information level will also be written into a rolling file
                    .WriteTo.Async(x => x.Logger(config =>
                        config
                            .MinimumLevel.Information()
                            .WriteTo.Async(x => x.File(
                                path: log.LoggerPath,
                                retainedFileCountLimit: 7,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(500)))
                    ))
                    // And create another logger so that logs at Fatal level will immediately send email
                    .WriteTo.Logger(config =>
                        config
                            .MinimumLevel.Fatal()
                            .WriteTo.Async(x => x.File(
                                path: log.CriticalLoggerPath,
                                retainedFileCountLimit: 1,
                                flushToDiskInterval: TimeSpan.FromMilliseconds(500)))
                    )).CreateLogger();
            }, CriticalAction: CriticalAlert);
            base.Init();
            return this;
        }
    }
}