namespace Kit
{
    public static class TimerExtensions
    {
        public static void Stop(this Timer Timer)
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite); //Stop timer
        }
    }
}