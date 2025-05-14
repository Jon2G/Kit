using Kit.Model;

namespace Kit.Controls.DateRange
{
    public class Rango : ModelBase
    {
        private DateTime? _Inicio;

        public DateTime? Inicio
        {
            get => _Inicio;
            set
            {
                if (_Inicio != value)
                {
                    _Inicio = value;
                    Raise(() => Inicio);
                    OnDateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private DateTime? _Fin;

        public DateTime? Fin
        {
            get => _Fin;
            set
            {
                if (_Fin != value)
                {
                    _Fin = value;
                    Raise(() => Fin);
                    OnDateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool? TodasLasFechasNull
        {
            get => _TodasLasFechas;
            set
            {
                if (value is bool b)
                {
                    this.TodasLasFechas = b;
                }
            }
        }

        private bool _SeleccionaFecha;

        public bool SeleccionaFecha
        {
            get => _SeleccionaFecha;
            set
            {
                _SeleccionaFecha = value;
                Raise(() => SeleccionaFecha);
            }
        }

        private bool _TodasLasFechas;

        public bool TodasLasFechas
        {
            get => _TodasLasFechas;
            set
            {
                _TodasLasFechas = value;
                Raise(() => TodasLasFechas);
                Raise(() => TodasLasFechasNull);

                OnDateChanged?.Invoke(this, EventArgs.Empty);
                SeleccionaFecha = !TodasLasFechas;
                //if (TodasLasFechas)
                //{
                //    Fin =
                //    Inicio = null;
                //}
                //else
                //{
                //    Inicio = DateTime.Now;
                //    Fin = DateTime.Now;
                //}
            }
        }

        public bool Cancelado { get; set; }
        public bool Excel { get; set; }

        public event EventHandler OnDateChanged;

        public Rango()
        {
            Excel = false;
            Cancelado = false;
            _Inicio = DateTime.Now;
            _Fin = DateTime.Now;
        }
    }
}