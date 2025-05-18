using Kit.Model;
using System.Collections.ObjectModel;

namespace Kit.Forms.Controls.NotificationBar
{
    public class Notificaciones : StaticModel<Notificaciones>
    {
        private static Notificaciones _Instance;
        public static Notificaciones Instance
        {
            get => _Instance;
            set
            {
                _Instance = value;
                OnGlobalPropertyChanged();
            }
        }

        public ObservableCollection<Notificacion> Elementos { get; private set; }
        public static Notificaciones Init()
        {
            NotificationBar.Notificaciones.Instance = new NotificationBar.Notificaciones();
            return Instance;
        }
        public Notificaciones()
        {
            this.Elementos = new ObservableCollection<Notificacion>();
        }
        ~Notificaciones()
        {

        }
        private async Task Notificar(Notificacion Notificacion)
        {
            await Device.InvokeOnMainThreadAsync(() =>
            {
                this.Elementos.Add(Notificacion);
            });
        }
        private async Task Remove(Notificacion notificacion)
        {
            await Device.InvokeOnMainThreadAsync(() =>
            {
                this.Elementos.Remove(notificacion);
            });
        }
    }
}
