namespace Kit.Controls.CrossBrush
{
    public abstract class CustomBackground<R, B, C>
        where B : CrossBrush<R, C>, new()
        where C : Color, new()
    {
        public B Brush { get; set; }
        public B DarkBrush { get; set; }
        private readonly string ResourceKey;

        public CustomBackground()
        {

        }
        public CustomBackground(string ResourceKey, string DarkResourceKey)
        {
            this.ResourceKey = ResourceKey;
            this.Brush = new B()
            {
                ResourceKey = ResourceKey
            };
            this.DarkBrush = new B()
            {
                ResourceKey = DarkResourceKey
            };
            this.Brush.LoadFromResource();
            this.DarkBrush.LoadFromResource();
        }

        public static CustomBackground<R, B, C> Get<T>(
            string ResourceKey, string DarkResourceKey)
        where T : CustomBackground<R, B, C>
        {
            var type = typeof(T);
            return (T)
                Activator.CreateInstance(type,
                    new object[] { ResourceKey, DarkResourceKey }
                    );
        }
    }
}
