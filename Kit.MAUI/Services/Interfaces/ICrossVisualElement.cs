namespace Kit.Services.Interfaces
{
    public interface ICrossVisualElement
    {
        public bool IsVisible { get; set; }
        public object BindingContext { get; set; }
    }
}
