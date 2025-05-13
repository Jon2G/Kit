namespace Kit.Services.Interfaces
{
    public interface IImageConverter
    {
        public byte[] ToBytes<T>(T image);
        public T ToImage<T>(byte[] image);
    }
}
