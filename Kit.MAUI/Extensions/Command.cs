using System.Reflection;
using System.Windows.Input;

namespace Kit.Extensions
{
    public class Command<T> : Command
    {
        public Command(Action<T> execute)
            : base(o =>
            {
                if (IsValidParameter(ref o))
                {
                    execute((T)o);
                }
            })
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }
        }

        public Command(Action<T> execute, Func<T, bool> canExecute)
            : base(o =>
            {
                if (IsValidParameter(ref o))
                {
                    execute((T)o);
                }
            }, o => IsValidParameter(ref o) && canExecute((T)o))
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
        }

        private static bool IsValidParameter(ref object o)
        {
            if (o != null)
            {
                // The parameter isn't null, so we don't have to worry whether null is a valid option
                if (o is not T)
                {
                    return false;
                }
                return true;
            }

            var t = typeof(T);

            // The parameter is null. Is T Nullable?
            if (Nullable.GetUnderlyingType(t) != null)
            {
                return true;
            }

            // Not a Nullable, if it's a value type then null is not valid
            return !t.GetTypeInfo().IsValueType;
        }
    }

    public class Command : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;
        private ICommand extractForApp;

        private event EventHandler _weakEventManager;

        public Command(Action<object> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
        }

        public Command(Action execute) : this(o => execute())
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
        }

        public Command(Action<object> execute, Func<object, bool> canExecute) : this(execute)
        {
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));

            _canExecute = canExecute;
        }

        public Command(Action execute, Func<bool> canExecute) : this(o => execute(), o => canExecute())
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            if (canExecute == null)
                throw new ArgumentNullException(nameof(canExecute));
        }

        public Command(ICommand extractForApp)
        {
            this.extractForApp = extractForApp;
        }

        public virtual bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(parameter);

            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { _weakEventManager += value; }
            remove { _weakEventManager -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void ChangeCanExecute()
        {
            _weakEventManager.Invoke(this, EventArgs.Empty);
        }
    }
}