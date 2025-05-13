
using System.Text.RegularExpressions;

namespace Kit
{
    public static partial class Helpers
    {
        public static async Task<byte[]> GetByteArray(this Stream input)
        {
            if (input is MemoryStream mem)
            {
                return mem.ToArray();
            }
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        //TODO: PORT CONTROLS
        //public static async Task<string> ToImageString(this CrossImage crossImage)
        //{
        //    Stream stream = await crossImage.ToStream();
        //    return await stream.ToImageString();
        //}
        public static async Task<string> ToImageString(this Stream stream)
        {
            return ToImageString(await stream.GetByteArray());
        }

        public static string ToImageString(this byte[] bytes)
        {
            string base64 = string.Concat("data:image/png;base64,", Convert.ToBase64String(bytes, 0, bytes.Length));
            return base64;
        }

        public static string EnLetra(this decimal Numero, string Leyenda = "M.N.", string strMoneda = "PESOS CON")
        {
            string dec;

            long entero = Convert.ToInt64(Math.Truncate(Numero));
            int decimales = Convert.ToInt32(Math.Round((Numero - entero) * 100, 2));
            //if (decimales > 0)
            //{
            //dec = " PESOS CON " + decimales.ToString() + "/100";
            dec = $" {strMoneda} {decimales:0,0} /100";
            //}
            ////Código agregado por mí
            //else
            //{
            //    //dec = " PESOS CON " + decimales.ToString() + "/100";
            //    dec = $" {strMoneda} {decimales:0,0} /100";
            //}
            string res = NumeroALetras(Convert.ToDouble(entero)) + dec;
            return $"{res} {Leyenda}";
        }

        private static string NumeroALetras(double value)
        {
            string num2Text; value = Math.Truncate(value);
            if (value == 0) num2Text = "CERO";
            else if (value == 1) num2Text = "UNO";
            else if (value == 2) num2Text = "DOS";
            else if (value == 3) num2Text = "TRES";
            else if (value == 4) num2Text = "CUATRO";
            else if (value == 5) num2Text = "CINCO";
            else if (value == 6) num2Text = "SEIS";
            else if (value == 7) num2Text = "SIETE";
            else if (value == 8) num2Text = "OCHO";
            else if (value == 9) num2Text = "NUEVE";
            else if (value == 10) num2Text = "DIEZ";
            else if (value == 11) num2Text = "ONCE";
            else if (value == 12) num2Text = "DOCE";
            else if (value == 13) num2Text = "TRECE";
            else if (value == 14) num2Text = "CATORCE";
            else if (value == 15) num2Text = "QUINCE";
            else if (value < 20) num2Text = "DIECI" + NumeroALetras(value - 10);
            else if (value == 20) num2Text = "VEINTE";
            else if (value < 30) num2Text = "VEINTI" + NumeroALetras(value - 20);
            else if (value == 30) num2Text = "TREINTA";
            else if (value == 40) num2Text = "CUARENTA";
            else if (value == 50) num2Text = "CINCUENTA";
            else if (value == 60) num2Text = "SESENTA";
            else if (value == 70) num2Text = "SETENTA";
            else if (value == 80) num2Text = "OCHENTA";
            else if (value == 90) num2Text = "NOVENTA";
            else if (value < 100) num2Text = NumeroALetras(Math.Truncate(value / 10) * 10) + " Y " + NumeroALetras(value % 10);
            else if (value == 100) num2Text = "CIEN";
            else if (value < 200) num2Text = "CIENTO " + NumeroALetras(value - 100);
            else if (value == 200 || value == 300 || value == 400 || value == 600 || value == 800) num2Text = NumeroALetras(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) num2Text = "QUINIENTOS";
            else if (value == 700) num2Text = "SETECIENTOS";
            else if (value == 900) num2Text = "NOVECIENTOS";
            else if (value < 1000) num2Text = NumeroALetras(Math.Truncate(value / 100) * 100) + " " + NumeroALetras(value % 100);
            else if (value == 1000) num2Text = "MIL";
            else if (value < 2000) num2Text = "MIL " + NumeroALetras(value % 1000);
            else if (value < 1000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000)) + " MIL";
                if (value % 1000 > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value % 1000);
                }
            }
            else if (value == 1000000)
            {
                num2Text = "UN MILLON";
            }
            else if (value < 2000000)
            {
                num2Text = "UN MILLON " + NumeroALetras(value % 1000000);
            }
            else if (value < 1000000000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000)) + " MILLONES ";
                if (value - Math.Truncate(value / 1000000) * 1000000 > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000) * 1000000);
                }
            }
            else if (value == 1000000000000) num2Text = "UN BILLON";
            else if (value < 2000000000000) num2Text = "UN BILLON " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            else
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if (value - Math.Truncate(value / 1000000000000) * 1000000000000 > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000);
                }
            }
            return num2Text;
        }

        public static string ExtractInitialsFromName(this string name, int length = 2)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            // Extract the first character out of each block of non-whitespace
            // exept name suffixes, e.g. Jr., III. The number of initials is not limited.
            name = Regex.Replace(name, @"(?i)(?:^|\s|-)+([^\s-])[^\s-]*(?:(?:\s+)(?:the\s+)?(?:jr|sr|II|2nd|III|3rd|IV|4th)\.?$)?", "$1").ToUpper();
            if (name.Length > length)
            {
                name = name.Substring(0, length);
            }

            return name;
        }

        public static void RunAsync(this Task task)
        {
            task.ContinueWith(t =>
            {
                Log.Logger.Error("Unexpected Error", t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}