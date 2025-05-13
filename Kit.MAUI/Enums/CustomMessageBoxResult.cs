namespace Kit.Enums
{
    //
    // Resumen:
    //     Specifies which message box button that a user clicks. System.Windows.CustomMessageBoxResult
    //     is returned by the Overload:System.Windows.MessageBox.Show method.
    public enum CustomMessageBoxResult
    {
        //
        // Resumen:
        //     The message box returns no result.
        None = 0,
        //
        // Resumen:
        //     The result value of the message box is OK.
        OK = 1,
        //
        // Resumen:
        //     The result value of the message box is Cancel.
        Cancel = 2,
        //
        // Resumen:
        //     The result value of the message box is Yes.
        Yes = 6,
        //
        // Resumen:
        //     The result value of the message box is No.
        No = 7
    }
}
