using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using View = Android.Views.View;
using ViewLifecycleEffectBase = Kit.Forms.Effects.ViewLifecycleEffect;
[assembly: ResolutionGroupName(ViewLifecycleEffectBase.EffectGroupName)]
[assembly: ExportEffect(typeof(Kit.Droid.Effects.ViewLifecycleEffect), ViewLifecycleEffectBase.EffectName)]
namespace Kit.Droid.Effects
{

    public class ViewLifecycleEffect : PlatformEffect
    {
        private View _nativeView;
        private ViewLifecycleEffectBase _viewLifecycleEffect;

        protected override void OnAttached()
        {
            _viewLifecycleEffect = Element.Effects.OfType<ViewLifecycleEffectBase>().FirstOrDefault();

            _nativeView = Control ?? Container;
            _nativeView.ViewAttachedToWindow += OnViewAttachedToWindow;
            _nativeView.ViewDetachedFromWindow += OnViewDetachedFromWindow;
        }

        protected override void OnDetached()
        {
            View view = Control ?? Container;
            _viewLifecycleEffect.RaiseUnloaded(Element);
            _nativeView.ViewAttachedToWindow -= OnViewAttachedToWindow;
            _nativeView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
        }

        private void OnViewAttachedToWindow(object sender, View.ViewAttachedToWindowEventArgs e) => _viewLifecycleEffect?.RaiseLoaded(Element);
        private void OnViewDetachedFromWindow(object sender, View.ViewDetachedFromWindowEventArgs e) => _viewLifecycleEffect?.RaiseUnloaded(Element);
    }
}

