using Kit.Forms.Extensions;
using Kit.Model;
using System.Text;

namespace Kit.Forms.Services
{
    public class IKeyboardListenerService : ModelBase, IDisposable
    {
        public static IKeyboardListenerService Current { get; private set; }
        private StringBuilder RecievedText;
        public string Code => RecievedText?.ToString();
        private MyTimer CountDown { get; set; }

        public event EventHandler<string> OnReciveCode;

        public event EventHandler<string> OnReciveCharacter;
        public Command<IKeyboardListenerService> OnKeyboardPluggedInChanged { get; set; }

        private bool _IsEnabled;

        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    Raise(() => IsEnabled);
                    Raise(() => IsDisabled);
                }
            }
        }


        private bool _IsKeyboardPluggedIn;

        public bool IsKeyboardPluggedIn
        {
            get => _IsKeyboardPluggedIn;
            set
            {
                if (_IsKeyboardPluggedIn != value)
                {
                    _IsKeyboardPluggedIn = value;
                    OnKeyboardPluggedInChanged?.Execute(this);
                    if (!value)
                    {
                        IsEnabled = false;
                    }
                }
            }
        }

        public bool IsDisabled => !IsEnabled;

        public IKeyboardListenerService(EventHandler<string> ReciveCode = null, EventHandler<string> ReviceCharacter = null, bool IsEnabled = true)
        {
            Current = this;
            RecievedText = new StringBuilder();
            this.IsEnabled = IsEnabled;
            this.OnReciveCharacter += ReviceCharacter;
            this.OnReciveCode += ReciveCode;
            if (this.OnReciveCode is null && this.OnReciveCharacter is null)
            {
                this.IsEnabled = false;
            }
            this.IsKeyboardPluggedIn = KeyboardVisibilityListener.KeyboardVisibilityState.Instance.IsKeyboardOpen;
        }

        public void SetIsKeyboardPluggedIn(bool isPluggedIn)
        {
            this.IsKeyboardPluggedIn = isPluggedIn;
        }

        ~IKeyboardListenerService()
        {
            Dispose();
        }

        public IKeyboardListenerService SetOnKeyboardPluggedInChanged(Command<IKeyboardListenerService> command)
        {
            this.OnKeyboardPluggedInChanged = command;
            return this;
        }

        public IKeyboardListenerService SetDelay(long WaitMillis)
        {
            this.RecievedText = new StringBuilder();
            CountDown = new MyTimer(TimeSpan.FromMilliseconds(WaitMillis), Confirm);
            return this;
        }

        private void Confirm()
        {
            this.CountDown.Stop();
            string code = RecievedText?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(code))
                OnReciveCode?.Invoke(this, code);
            this.RecievedText = new StringBuilder();
        }

        public void OnKeyUp(char character)
        {
            if (!IsEnabled)
            {
                return;
            }

            string text = character.ToString();
            if (string.IsNullOrEmpty(text) || text == "\0")
            {
                return;
            }

            if (text == "\n" && !string.IsNullOrEmpty(Code))
            {
                OnReciveCode?.Invoke(this, Code);
                RecievedText = new StringBuilder();
            }
            else
            {
                this.OnReciveCharacter?.Invoke(this, text);
                RecievedText.Append(character);
            }
            CountDown?.Restart();
            Raise(() => Code);
        }
        public override void Dispose()
        {
            base.Dispose();
            OnReciveCharacter = null;
            OnReciveCode = null;
            RecievedText?.Clear();
            RecievedText = null;
            CountDown = null;
            Current = null;
        }
    }
}