namespace Kit.Controls.CrossBrush
{
    public static class GradientBrushConstructor
    {

        public static CustomBackground<R, B, C>
            Get<T, R, B, C>(
                string MainBackround = "MainBackround",
                string DarkBackround = "DarkBackround")
            where B : CrossBrush<R, C>, new()
            where C : Color, new()
            where T : CustomBackground<R, B, C>
        {
           return CustomBackground<R, B, C>.Get<T>(MainBackround, DarkBackround);
        }



    }
}
