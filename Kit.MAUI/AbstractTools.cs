using Kit.Dialogs;
using Kit.Enums;
using Kit.Services.BarCode;
using Kit.Services.Interfaces;
using System.Diagnostics;

namespace Kit
{
    public abstract class AbstractTools
    {
        
        private Lazy<ISynchronizeInvoke> _SynchronizeInvoke =
            new Lazy<ISynchronizeInvoke>(() => TinyTypeContainer.Container.Get<ISynchronizeInvoke>());
        public ISynchronizeInvoke SynchronizeInvoke => _SynchronizeInvoke.Value;

        private Lazy<IScreenManager> _ScreenManager =
            new Lazy<IScreenManager>(() => TinyTypeContainer.Container.Get<IScreenManager>());
        public IScreenManager ScreenManager => _ScreenManager.Value;

        private Lazy<IBarCodeBuilder> _BarCodeBuilder =
            new Lazy<IBarCodeBuilder>(() => TinyTypeContainer.Container.Get<IBarCodeBuilder>());
        public IBarCodeBuilder BarCodeBuilder => _BarCodeBuilder.Value;

        private Lazy<IDialogs> _Dialogs =
            new Lazy<IDialogs>(() => TinyTypeContainer.Container.Get<IDialogs>());
        public IDialogs Dialogs => _Dialogs.Value;
        private Lazy<Kit.Controls.CrossImage.CrossImageExtensions> _ImageExtensions =
            new Lazy<Kit.Controls.CrossImage.CrossImageExtensions>(() => TinyTypeContainer.Container.Get<Kit.Controls.CrossImage.CrossImageExtensions>());
        public Kit.Controls.CrossImage.CrossImageExtensions ImageExtensions => _ImageExtensions.Value;

        public virtual string LibraryPath => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public virtual string TemporalPath => Path.Combine(LibraryPath, "..", "tmp");

        protected AbstractTools()
        {
        }

        public static AbstractTools Instance => Tools.Instance;

        public virtual AbstractTools Init()
        {
            return this;
        }
        public virtual void CriticalAlert(string ex)
        {
            this.Dialogs?.CustomMessageBox.ShowOK(ex, "Alerta", "Entiendo");
        }

        private static readonly Lazy<bool> _IsInDesingMode = new Lazy<bool>(IsDesigning, System.Threading.LazyThreadSafetyMode.PublicationOnly);
        public static bool IsInDesingMode
        {
            get
            {
                bool ret = _IsInDesingMode.Value;
                return ret;
            }
        }

        public abstract RuntimePlatform RuntimePlatform { get; }

        private static bool IsDesigning()
        {
            //if (Device.RuntimePlatform == Device.UWP)
            //{
            //    return false;
            //}

            string name = Process.GetCurrentProcess().ProcessName;
            name = name?.Trim()?.ToUpper();
            if (name == "XDESPROC" || name == "DEVENV")
            {
                return true;
            }
            // MessageBox.Show(name);
            return false;
        }
    }
}