using Foundation;
using Kit.iOS.Effects;
using Microsoft.Maui.Controls.Platform;
using UIKit;

using EffectsBase = Kit.Forms.Effects;

[assembly: ResolutionGroupName(Kit.Forms.Effects.ViewLifecycleEffect.EffectGroupName)]
[assembly: ExportEffect(typeof(ViewLifecycleEffect), Kit.Forms.Effects.ViewLifecycleEffect.EffectName)]

namespace Kit.iOS.Effects
{
    public class ViewLifecycleEffect : PlatformEffect
    {
        private const NSKeyValueObservingOptions ObservingOptions =
            NSKeyValueObservingOptions.New | NSKeyValueObservingOptions.Old |
            NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.OldNew | NSKeyValueObservingOptions.Prior;

        private EffectsBase.ViewLifecycleEffect _viewLifecycleEffect;
        private IDisposable _isLoadedObserverDisposable;

        protected override void OnAttached()
        {
            _viewLifecycleEffect = Element.Effects.OfType<EffectsBase.ViewLifecycleEffect>().FirstOrDefault();

            UIView nativeView = Control ?? Container;
            _isLoadedObserverDisposable = nativeView?.AddObserver("superview", ObservingOptions, IsViewLoadedObserver);

        }

        protected override void OnDetached()
        {
            _viewLifecycleEffect.RaiseUnloaded(Element);
            _isLoadedObserverDisposable.Dispose();
        }

        private void IsViewLoadedObserver(NSObservedChange nsObservedChange)
        {
            if (!nsObservedChange.NewValue.Equals(NSNull.Null))
            {
                _viewLifecycleEffect?.RaiseLoaded(Element);
            }
            else
            {
                if (nsObservedChange.OldValue != null && !nsObservedChange.OldValue.Equals(NSNull.Null))
                {
                    _viewLifecycleEffect?.RaiseUnloaded(Element);
                }
            }


        }
    }
}
