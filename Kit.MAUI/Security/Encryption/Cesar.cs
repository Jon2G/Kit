using System.Text;

namespace Kit.Security.Encryption
{
    public class Cesar : Encryption
    {
        public readonly int Key;
        public Cesar(int Key = 777) : base(Encoding.UTF8)
        {
            this.Key = Key;
        }
        public Cesar(Encoding Encoding, int Key = 777) : base(Encoding)
        {
            this.Key = Key;
        }

        public override byte[] Encrypt(string Value)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return null;
            }
            long nLargo;
            int n;
            char cCaracter;
            string NewCadena;
            int NewNumero;
            nLargo = Value.Trim().Length;
            NewCadena = "";
            NewNumero = Key / 256 * 4;
            for (n = 0; n < nLargo; n++)
            {
                cCaracter = (char)(Value.Substring(n, 1)[0] + NewNumero);
                NewCadena += cCaracter;
            }
            return ToBytes(NewCadena);
        }

        public override byte[] Decrypt(byte[] Array)
        {
            if (Array is null)
            {
                return null;
            }
            string Value = ToString(Array);
            long nLargo;
            int n;
            char cCaracter;
            string NewCadena;
            int NewNumero;
            nLargo = Value.Trim().Length;
            NewCadena = "";
            NewNumero = Key / 256 * 4;
            for (n = 0; n < nLargo; n++)
            {
                cCaracter = (char)(Value.Substring(n, 1)[0] -NewNumero);
                NewCadena += cCaracter;
            }

            return ToBytes(NewCadena);
        }


    }
}
