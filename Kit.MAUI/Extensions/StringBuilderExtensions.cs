using System.Text;

// ReSharper disable once CheckNamespace
namespace Kit
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendLine<T>(this StringBuilder sb, Func<IEnumerable<T>> values)
        {
            foreach (T value in values.Invoke())
            {
                sb.AppendLine(value.ToString());
            }
            return sb;
        }
        public static StringBuilder Append<T>(this StringBuilder sb, Func<IEnumerable<T>> values)
        {
            foreach (T value in values.Invoke())
            {
                sb.Append(value.ToString());
            }
            return sb;
        }
        public static StringBuilder AppendLine<T>(this StringBuilder sb, Func<T> value) => sb.AppendLine(value.Invoke().ToString());
        public static StringBuilder Append<T>(this StringBuilder sb, Func<T> value) => sb.Append(value.Invoke().ToString());
        public static StringBuilder TrimEnd(this StringBuilder sb)
        {
            int index = sb.Length - 1;
            if (index < 0) { return sb; }
            char last = sb[index];
            while (last == '\n' || last == '\r' && index > 0)
            {
                index = sb.Length - 1;
                if (index < 0) { return sb; }
                sb.Remove(index, 1);
                index--;
                last = sb[index];
            }
            return sb;
        }

        public static char Last(this StringBuilder sb)
        {
            return sb.ToString(sb.Length - 1, 1)[0];
        }
        public static StringBuilder RemoveLast(this StringBuilder sb)
        {
            return sb.Remove(sb.Length - 1, 1);
        }
        public static bool EndsWith(this StringBuilder sb, string test)
        {
            if (sb.Length < test.Length)
                return false;

            string end = sb.ToString(sb.Length - test.Length, test.Length);
            return end.Equals(test);
        }

    }
}