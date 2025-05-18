namespace Kit.Forms.Services.Interfaces
{
    public interface IUpdateWidget
    {
        /// <summary>
        /// Request the specified widget to Update
        /// </summary>
        /// <param name="AppWidgetProviderFullClassName">On Android it must match widgetprovider class name, On ios it works as an identifier for you to override implementation on AppDelegate</param>
        public abstract void UpdateWidget(string AppWidgetProviderFullClassName=null);
    }
}
