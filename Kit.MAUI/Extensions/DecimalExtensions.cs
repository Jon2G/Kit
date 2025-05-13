namespace Kit
{
    public static class DecimalExtensions
    {
        public static int CountDecimalDigits(this decimal n)
        {
            return n.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    //.TrimEnd('0') uncomment if you don't want to count trailing zeroes
                    .SkipWhile(c => c != '.')
                    .Skip(1)
                    .Count();
        }
    }
}
