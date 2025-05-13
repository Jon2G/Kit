namespace Kit
{
    public static class Elevator
    {
        public static T Elevate<T>(this object Lower)
            where T : new()
        {
            var ElevatedType = typeof(T);
            var LowerType = Lower.GetType();
            if (LowerType.IsSubclassOf(ElevatedType))
            {
                throw new InvalidCastException($"{LowerType.Name} is not subclass of {ElevatedType.Name}");
            }
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Lower);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}