namespace Kit
{
    public static class BytesConverter
    {
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static double ToSize(this long value, SizeUnits unit)
        {
            return (value / (double)Math.Pow(1024, (Int64)unit));
        }
    }
}