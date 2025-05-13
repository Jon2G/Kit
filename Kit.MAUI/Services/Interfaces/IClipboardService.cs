namespace Kit.Services.Interfaces
{
    public interface IClipboardService
    {
        Task SetText(string text);
        Task<string> GetText();
    }
}
