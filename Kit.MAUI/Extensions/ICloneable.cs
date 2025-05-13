

// ReSharper disable once CheckNamespace
namespace Kit
{
    public interface ICloneable<out T> : ICloneable
    {
        new T Clone();
    }
}
