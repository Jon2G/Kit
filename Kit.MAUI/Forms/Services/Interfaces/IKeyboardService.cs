namespace Kit.Forms.Services.Interfaces
{
    public interface IKeyboardService : Forms9Patch.IKeyboardService
    {
        public void Toggle();
        public void Show();
        public void Close();
    }
}
