using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Java.Interop;
using Java.Util;
using Rect = Android.Graphics.Rect;
using View = Android.Views.View;

namespace Kit.Droid.Utils
{
    [Preserve(AllMembers = true)]
    public class KeyboardUtils : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private const int MAGIC_NUMBER = 200;

        private SoftKeyboardToggleListener mCallback;
        private View mRootView;
        private bool? prevValue = null;
        private float mScreenDensity;

        private static Android.Runtime.JavaDictionary<SoftKeyboardToggleListener, KeyboardUtils> sListenerMap = new((new HashMap()).Handle, Android.Runtime.JniHandleOwnership.DoNotRegister);


        public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();

        public interface SoftKeyboardToggleListener
        {
            void onToggleSoftKeyboard(bool isVisible);
        }



        /**
         * Add a new keyboard listener
         * @param act calling activity
         * @param listener callback
         */
        public static void addKeyboardToggleListener(Activity act, SoftKeyboardToggleListener listener)
        {
            removeKeyboardToggleListener(listener);

            sListenerMap.Add(listener, new KeyboardUtils(act, listener));
        }

        /**
         * Remove a registered listener
         * @param listener {@link SoftKeyboardToggleListener}
         */
        public static void removeKeyboardToggleListener(SoftKeyboardToggleListener listener)
        {
            if (sListenerMap.ContainsKey(listener))
            {
                KeyboardUtils k = sListenerMap[listener];
                k.removeListener();

                sListenerMap.Remove(listener);
            }
        }

        /**
         * Remove all registered keyboard listeners
         */
        public static void removeAllKeyboardToggleListeners()
        {
            foreach (SoftKeyboardToggleListener l in sListenerMap.Keys)
            {
                sListenerMap[l].removeListener();
            }

            sListenerMap.Clear();
        }

        /**
         * Manually toggle soft keyboard visibility
         * @param context calling context
         */
        public static void toggleKeyboardVisibility(Context context)
        {
            InputMethodManager inputMethodManager =
                (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            if (inputMethodManager != null)
                inputMethodManager.ToggleSoftInput(ShowFlags.Forced, 0);
        }

        /**
         * Force closes the soft keyboard
         * @param activeView the view with the keyboard focus
         */
        public static void forceCloseKeyboard(View activeView)
        {
            InputMethodManager inputMethodManager =
                (InputMethodManager)activeView.Context.GetSystemService(Context.InputMethodService);
            if (inputMethodManager != null)
                inputMethodManager.HideSoftInputFromWindow(activeView.WindowToken, 0);
        }

        private void removeListener()
        {
            mCallback = null;

            mRootView.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
        }

        public void OnGlobalLayout()
        {
            Rect r = new Rect();
            mRootView.GetWindowVisibleDisplayFrame(r);

            int heightDiff = mRootView.RootView.Height - (r.Bottom - r.Top);
            float dp = heightDiff / mScreenDensity;
            bool isVisible = dp > MAGIC_NUMBER;

            if (mCallback != null && (prevValue == null || isVisible != prevValue))
            {
                prevValue = isVisible;
                mCallback.onToggleSoftKeyboard(isVisible);
            }
        }

        public void SetJniIdentityHashCode(int value)
        {

        }

        public void SetPeerReference(JniObjectReference reference)
        {
            throw new NotImplementedException();
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
            throw new NotImplementedException();
        }


        public void DisposeUnlessReferenced()
        {
            throw new NotImplementedException();
        }

        public void Disposed()
        {
            throw new NotImplementedException();
        }

        public void Finalized()
        {
            throw new NotImplementedException();
        }


        private KeyboardUtils(Activity act, SoftKeyboardToggleListener listener)
        {
            mCallback = listener;

            mRootView = ((ViewGroup)act.FindViewById(Android.Resource.Id.Content)).GetChildAt(0);
            mRootView.ViewTreeObserver.AddOnGlobalLayoutListener(this);

            mScreenDensity = act.Resources.DisplayMetrics.Density;
        }
    }
}
