namespace Kit.Services.Interfaces
{
    public interface ICrossWindow : ICrossVisualElement
    {
        Task Close();

        Task Show();

        Task ShowDialog();
        public void CrossOnAppearing();
    }
}