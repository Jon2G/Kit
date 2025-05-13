namespace Kit.Dialogs
{
    public interface ILoading : IDisposable
    {
        IDisposable Show(string Text = "Cargando...");
    }
}