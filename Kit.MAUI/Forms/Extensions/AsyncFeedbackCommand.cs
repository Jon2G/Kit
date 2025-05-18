using AsyncAwaitBestPractices.MVVM;
using Kit.Forms.Extensions;

namespace Kit.Forms
{
    public class AsyncFeedbackCommand<T> : AsyncCommand<T>
    {
        public AsyncFeedbackCommand(Func<T?, Task> execute, Func<object?, bool>? canExecute = null, Action<Exception>? onException = null, bool continueOnCapturedContext = false) :
            base((args) => FeedbackExecute(execute, args), canExecute, onException, continueOnCapturedContext)
        {
        }
        public AsyncFeedbackCommand(Action<T?> execute, Func<object?, bool>? canExecute = null, Action<Exception>? onException = null, bool continueOnCapturedContext = false) :
            base((args) => FeedbackExecute(execute, args), canExecute, onException, continueOnCapturedContext)
        {
        }
        private static async Task FeedbackExecute(Action<T?> execute, T args)
        {
            await AsyncFeedbackCommand.Feedback();
            execute.Invoke(args);
        }
        private static async Task FeedbackExecute(Func<T?, Task> execute, T args)
        {
            await AsyncFeedbackCommand.Feedback();
            await execute(args);
        }
    }
    public class AsyncFeedbackCommand : AsyncCommand
    {
        public AsyncFeedbackCommand(Action execute, Func<object, bool> canExecute = null,
            Action<Exception> onException = null, bool continueOnCapturedContext = false) :
            this(() => FeedbackExecute(execute), canExecute, onException, continueOnCapturedContext)
        {

        }
        public AsyncFeedbackCommand(Func<Task> execute, Func<object, bool> canExecute = null, Action<Exception> onException = null, bool continueOnCapturedContext = false) :
            base(() => FeedbackExecute(execute), canExecute, onException, continueOnCapturedContext)
        {
        }

        internal static async Task Feedback()
        {
            await Task.Yield();
            if (await Permisos.CanVibrate())
            {
                HapticFeedback.Perform(HapticFeedbackType.Click);
            }
        }

        private static async Task FeedbackExecute(Action execute)
        {
            await Feedback();
            execute.Invoke();
        }
        private static async Task FeedbackExecute(Func<Task> execute)
        {
            await Feedback();
            await execute();
        }
    }
}
