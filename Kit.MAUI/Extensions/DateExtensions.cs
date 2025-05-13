namespace Kit
{
    public static class DateExtensions
    {
        public static DateTime MexicoCityCurrentDateTime()
        {
            //if (DateTime.UtcNow.IsDaylightSavingTime())
            //{
            //    return DateTime.UtcNow.AddHours(-5);
            //}
            return DateTime.UtcNow.AddHours(-6);
        }

        /// <summary>
        /// Gets current time of a time zone no matter if its on a server
        /// </summary>
        /// <param name="tzi"></param>
        /// <returns></returns>
        public static DateTime CurrentTimeOf(this TimeZoneInfo tzi)
        {
            DateTime utcTime = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi); // convert from utc to local
        }

        public static DateTime GetNextOrToday(this DayOfWeek day)
        {
            DateTime date = DateTime.Today;
            while (date.DayOfWeek != day)
            {
                date = date.AddDays(+1);
            }
            return date;
        }
        public static DateTime GetNearest(this DayOfWeek day)
        {
            DateTime date = DateTime.Today;
            while (date.DayOfWeek != day)
            {
                if (date.DayOfWeek > day)
                {
                    date = date.AddDays(-1);
                }
                else
                {
                    date = date.AddDays(+1);
                }
            }
            return date;
        }


        public static string GetDayName(this DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return "Domingo";

                case DayOfWeek.Monday:
                    return "Lunes";

                case DayOfWeek.Tuesday:
                    return "Martes";

                case DayOfWeek.Wednesday:
                    return "Miércoles";

                case DayOfWeek.Thursday:
                    return "Jueves";

                case DayOfWeek.Friday:
                    return "Viernes";

                case DayOfWeek.Saturday:
                    return "Sábado";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string Mes(this int Mes)
        {
            switch (Mes)
            {
                case 1:
                    return "Enero";

                case 2:
                    return "Febrero";

                case 3:
                    return "Marzo";

                case 4:
                    return "Abril";

                case 5:
                    return "Mayo";

                case 6:
                    return "Junio";

                case 7:
                    return "Julio";

                case 8:
                    return "Agosto";

                case 9:
                    return "Septiembre";

                case 10:
                    return "Octubre";

                case 11:
                    return "Noviembre";

                case 12:
                    return "Diciembre";

                default:
                    throw new ArgumentException();
            }
        }



        /// <summary>
        /// Devuelve un saludo según la hora indicada.
        /// 00:00-11:59 Buenos días
        /// 12:00-18:59 Buenas tardes
        /// 19:00-23:59 Buenas noches
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string Saludo(this DateTime date)
        {
            int hora = date.Hour;
            int minutos = date.Minute;
            if (hora <= 11 && minutos <= 59)//todas las horas menores a las 11:59
            {
                return "Buenas días";
            }
            if (hora <= 18 && minutos <= 59)
            {
                return "Buenas tardes";
            }
            return "Buenas noches";
        }
    }
}