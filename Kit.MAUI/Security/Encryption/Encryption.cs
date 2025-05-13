using System.Text;

namespace Kit.Security.Encryption
{
    public abstract class Encryption
    {
        protected readonly Encoding Encoding;
        public Encryption(Encoding Encoding)
        {
            this.Encoding = Encoding;
        }
        public abstract byte[] Encrypt(string Value);
        public virtual string UnEncrypt(string value)
        {
            return ToString(Decrypt(ToBytes(value)));
        }
        public virtual byte[] ToBytes(string Value)
        {
            return Encoding.GetBytes(Value);

        }
        public abstract byte[] Decrypt(byte[] Array);
        public string ToString(byte[] Array)
        {
            if (Array is null)
            {
                return string.Empty;
            }
            return Encoding.GetString(Array);
        }

    }
}
