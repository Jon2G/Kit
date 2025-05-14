namespace Kit.Controls.DateRange
{
    public class ModeloRango : Rango
    {
        private DateTime _MinDate;
        private DateTime _InicioMaxDate;

        public DateTime InicioMaxDate
        {
            set
            {
                _InicioMaxDate = value;
                Raise(() => InicioMaxDate);
            }
            get => _InicioMaxDate;
        }

        private DateTime _FinMaxDate;

        public DateTime FinMaxDate
        {
            set
            {
                _FinMaxDate = value;
                Raise(() => FinMaxDate);
            }
            get => _FinMaxDate;
        }

        public DateTime MinDate
        {
            set
            {
                _MinDate = value;
                Raise(() => MinDate);
            }
            get => _MinDate;
        }

        public ModeloRango() : base()
        {
            FinMaxDate =
            InicioMaxDate = DateTime.Now;
            Fin = FinMaxDate;
            Inicio = DateTime.Now.AddMonths(-1);
            PropertyChanged += ModeloRango_PropertyChanged;
        }

        private void ModeloRango_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Inicio):
                    if (Inicio != null)
                    {
                        if (Fin < Inicio)
                        {
                            Fin = Inicio;
                        }
                        MinDate = (DateTime)Inicio;
                    }
                    break;

                case nameof(Fin):
                    if (Inicio != null)
                    {
                        if (Inicio > Fin)
                        {
                            Fin = Inicio;
                        }
                    }
                    break;
            }
        }
    }
}