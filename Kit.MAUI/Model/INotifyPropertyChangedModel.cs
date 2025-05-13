using AsyncAwaitBestPractices;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Kit.Model
{
    [Preserve(AllMembers = true)]
    public abstract class INotifyPropertyChangedModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        //[Obsolete("Use Raise para mejor rendimiento evitando la reflección")]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, args);
        }

        #endregion INotifyPropertyChanged

        #region PerfomanceHelpers

        protected bool RaiseIfChanged<T>(ref T? backingField, T new_value, [CallerMemberName] string propertyName = null) where T : IComparable
        {
            if (new_value is null || new_value.CompareTo(backingField) != 0)
            {
                backingField = new_value;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
        public INotifyPropertyChangedModel Raise<T>(Expression<Func<T>> propertyExpression)
        {
            AsyncRaise<T>(propertyExpression).SafeFireAndForget();
            return this;
        }
        protected async Task AsyncRaise<T>(Expression<Func<T>> propertyExpression)
        {
            await Task.Yield();
            if (this.PropertyChanged != null)
            {
                MemberExpression body = propertyExpression.Body as MemberExpression;
                if (body == null)
                    throw new ArgumentException("'propertyExpression' should be a member expression");

                //ConstantExpression expression = body.Expression as ConstantExpression;
                //if (expression == null)
                //{
                //    throw new ArgumentException("'propertyExpression' body should be a constant expression");
                //}

                object target = Expression.Lambda(body.Expression).Compile().DynamicInvoke();
                if (target is null)
                    return;
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(body.Member.Name);
                try
                {
                    PropertyChanged(target, e);
                    OnPropertyRaised(target, e.PropertyName);
                }
                catch (Exception)
                {
                    //  Log.Logger.Error(ex, "On RAISE ");
                }
            }
        }

        protected void Raise<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (Expression<Func<T>> propertyExpression in propertyExpressions)
            {
                Raise<T>(propertyExpression);
            }
        }

        protected virtual void OnPropertyRaised(object target, string PropertyName)
        {
        }

        #endregion PerfomanceHelpers

    }
}
