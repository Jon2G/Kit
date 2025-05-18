using Xamarin.Forms;

namespace Kit
{
    public static class ControlExtensions
    {
        public static T FindParent<T>(this Xamarin.Forms.Element element)
        {
            Element parent = element;
            while (parent != null)
            {
                parent = parent.Parent;
                if(parent is T found)
                {
                    return found;
                }
            }
            return default(T);
        }
    }
}
