using System.Numerics;

namespace Kit.Security.Encryption
{
    public static class Modulus
    {
        public static BigInteger Mod(BigInteger ba, BigInteger bb)
        {
            return ((ba * bb) + ba) % bb;
        }
        public static ulong Mod(ulong a, ulong b)
        {
            return ((a * b) + a) % b;
        }
        public static int Mod(int a, int b)
        {
            return (Math.Abs(a * b) + a) % b;
        }
    }
}
