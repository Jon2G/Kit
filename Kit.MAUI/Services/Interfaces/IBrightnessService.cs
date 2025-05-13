namespace Kit.Services.Interfaces
{
    public interface IBrightnessService
    {
        void SetBrightness(float factor);
        float GetBrightness();
    }
}
