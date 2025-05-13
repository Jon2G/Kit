using System.Diagnostics;

namespace Kit
{
    public partial class Tools
    {
        public static bool IsInited => currentInstance != null;

        public static void Set(AbstractTools Instance)
        {
            currentInstance = Instance;
        }

        private static AbstractTools currentInstance;

        public static AbstractTools Instance
        {
            get
            {
                if (currentInstance == null)
                {
                    return null;
                }
                //throw new ArgumentException("Please Init Plugin.Xamarin.Tools before using it");

                return currentInstance;
            }
            set => currentInstance = value;
        }

        private static bool? _Debugging = null;
        public static bool Debugging => _Debugging ?? Debugger.IsAttached;

        public static bool SetDebugging(bool? debugging = null)
        {
            Tools._Debugging = debugging;
            return Debugging;
        }

        protected static void BaseInit()
        {
            if (Tools.Instance.RuntimePlatform != Enums.RuntimePlatform.WPF)
            {
                DirectoryInfo TempDirectory = new DirectoryInfo(Instance.TemporalPath);
                if (!TempDirectory.Exists)
                {
                    TempDirectory.Create();
                }
            }
        }
    }
}