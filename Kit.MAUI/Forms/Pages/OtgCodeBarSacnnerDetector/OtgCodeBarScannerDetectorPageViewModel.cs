using AsyncAwaitBestPractices;
using Kit.Forms.Services;
using Kit.Model;

namespace Kit.Forms.Pages.OtgCodeBarSacnnerDetector
{
    internal class OtgCodeBarScannerDetectorPageViewModel : ModelBase
    {
        private string _Code;

        public string Code
        {
            get => _Code;
            set
            {
                _Code = value;
                Raise(() => Code);
            }
        }

        private string _Status;

        public string Status
        {
            get => _Status;
            set
            {
                _Status = value;
                Raise(() => Status);
            }
        }

        private Services.IKeyboardListenerService IKeyboardListenerService;
        private View BeginView { get; set; }
        private View CenterView { get; set; }
        private View EndView { get; set; }

        internal void Init(View BeginView, View CenterView, View EndView)
        {
            this.BeginView = BeginView;
            this.CenterView = CenterView;
            this.EndView = EndView;
            IKeyboardListenerService = new Services.IKeyboardListenerService((e, s) => ReadCode(e, s).SafeFireAndForget(), ReadCharacter, IsEnabled: true);
            IKeyboardListenerService.OnKeyboardPluggedInChanged =
                new Command<IKeyboardListenerService>(ListenerService_OnKeyboardPluggedInChanged);
            ListenerService_OnKeyboardPluggedInChanged(IKeyboardListenerService);
        }

        private void ListenerService_OnKeyboardPluggedInChanged(IKeyboardListenerService listener)
        {
            Status = IKeyboardListenerService.IsKeyboardPluggedIn ?
                "Tome una lectura" :
                "Por favor conecte su lector de código de barras";
        }

        private async Task ReadCode(object sender, string Code)
        {
            await Task.Yield();
            this.Code = Code;
            this.CenterView.TranslateTo(0, 50).SafeFireAndForget();
            this.CenterView.TranslateTo(0, 0).SafeFireAndForget();
        }



        private void ReadCharacter(object sender, string character)
        {
            IKeyboardListenerService service = (IKeyboardListenerService)sender;
            this.Code = service.Code;

            this.BeginView.TranslateTo(0, 50).SafeFireAndForget();
            this.EndView.TranslateTo(0, 50).SafeFireAndForget();
            this.BeginView.TranslateTo(0, 0).SafeFireAndForget();
            this.EndView.TranslateTo(0, 0).SafeFireAndForget();
        }
    }
}