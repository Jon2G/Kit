namespace Kit.Forms.Validations
{
    [Preserve(AllMembers = true)]
    public interface INotifyScrollToProperty
    {
        event ScrollToPropertyHandler ScrollToProperty;
    }
    [Preserve(AllMembers = true)]
    public delegate void ScrollToPropertyHandler(string PropertyName);
}
