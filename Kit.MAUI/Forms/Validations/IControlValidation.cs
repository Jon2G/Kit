namespace Kit.Forms.Validations
{
    [Preserve(AllMembers = true)]
    public interface IControlValidation
    {
        bool HasError { get; }
        string ErrorMessage { get; }
        bool ShowErrorMessage { get; set; }
    }
}
