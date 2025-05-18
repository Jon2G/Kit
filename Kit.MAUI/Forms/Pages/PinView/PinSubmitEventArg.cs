namespace Kit.Forms.Pages.PinView
{
    public class PinSubmitEventArg
    {
        public PinSubmitEventArg(object sender, string pin)
        {
            Source = sender;
            Pin = pin;
        }
        public object Source { get; private set; }
        public string Pin { get; private set; }
    }
}
