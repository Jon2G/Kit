﻿using Kit.Forms.Extensions;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;

namespace Kit.Forms
{
    public class AsyncFeedbackCommand<T> : AsyncCommand<T>
    {
        public AsyncFeedbackCommand(Func<T?, Task> execute, Func<object?, bool>? canExecute = null, Action<Exception>? onException = null, bool continueOnCapturedContext = false, bool allowsMultipleExecutions = true) :
            base((args) => FeedbackExecute(execute, args), canExecute, onException, continueOnCapturedContext, allowsMultipleExecutions)
        {
        }
        public AsyncFeedbackCommand(Action<T?> execute, Func<object?, bool>? canExecute = null, Action<Exception>? onException = null, bool continueOnCapturedContext = false, bool allowsMultipleExecutions = true) :
            base((args) => FeedbackExecute(execute, args), canExecute, onException, continueOnCapturedContext, allowsMultipleExecutions)
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
            Action<Exception> onException = null, bool continueOnCapturedContext = false,
            bool allowsMultipleExecutions = true) :
            this(() => FeedbackExecute(execute), canExecute, onException, continueOnCapturedContext, allowsMultipleExecutions)
        {

        }
        public AsyncFeedbackCommand(Func<Task> execute, Func<object, bool> canExecute = null, Action<Exception> onException = null, bool continueOnCapturedContext = false, bool allowsMultipleExecutions = true) :
            base(() => FeedbackExecute(execute), canExecute, onException, continueOnCapturedContext, allowsMultipleExecutions)
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
