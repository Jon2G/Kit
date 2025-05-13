namespace Kit
{
    public class CircularList<T> : List<T>
    {
        private int Index;

        public CircularList(T[] items) : base(items)
        {

        }
        public CircularList() : this(0) { }

        public CircularList(int index)
        {
            if (index < 0 || index >= Count)
                throw new Exception(string.Format("Index must between {0} and {1}", 0, Count));

            Index = index;
        }

        public T Current()
        {
            return this[Index];
        }

        public T Next()
        {
            Index++;
            Index %= Count;

            return this[Index];
        }

        public T Previous()
        {
            Index--;
            if (Index < 0)
                Index = Count - 1;

            return this[Index];
        }

        public void Reset()
        {
            Index = 0;
        }

        public void MoveToEnd()
        {
            Index = Count - 1;
        }

    }
}
