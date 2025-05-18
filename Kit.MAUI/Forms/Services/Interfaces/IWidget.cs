using TinyTypeContainer;

namespace Kit.Forms.Services.Interfaces
{
    public abstract class IWidget
    {
        public abstract string AppWidgetProviderFullClassName { get; }
        public static void UpdateWidget(string AppWidgetProviderFullClassName)
        {
            IUpdateWidget iWidget = Container.Get<IUpdateWidget>();
            iWidget?.UpdateWidget(AppWidgetProviderFullClassName);
        }

        public static void UpdateWidget(IWidget widget) => UpdateWidget(widget.AppWidgetProviderFullClassName);
    }
}
