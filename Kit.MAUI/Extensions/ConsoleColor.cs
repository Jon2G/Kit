namespace Kit
{
    public static class ConsoleColor
    {
        public static string ToHex(this System.ConsoleColor color)
        {
            switch (color)
            {
                case System.ConsoleColor.White:
                    return "#FFFFFF";
                case System.ConsoleColor.Black:
                    return "#000000";
                case System.ConsoleColor.DarkMagenta:
                    return "#702963";
                case System.ConsoleColor.Blue:
                    return "#0000FF";
                case System.ConsoleColor.DarkBlue:
                    return "#000099";
                case System.ConsoleColor.DarkCyan:
                    return "#008b8b";
                case System.ConsoleColor.Cyan:
                    return "#00FFFF";
                case System.ConsoleColor.DarkGreen:
                    return "#006400";
                case System.ConsoleColor.Green:
                    return "#00FF00";
                case System.ConsoleColor.DarkGray:
                    return "#a9a9a9";
                case System.ConsoleColor.Gray:
                    return "#808080";
            }

            throw new NotImplementedException();
        }
    }
}
