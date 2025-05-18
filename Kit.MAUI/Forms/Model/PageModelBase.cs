using FreshMvvm.Maui;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

// ReSharper disable CheckNamespace

namespace Kit.Model
{
    [Preserve(AllMembers = true)]
    public class PageModelBase : FreshBasePageModel
    {
        #region INotifyPropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;
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

        protected async void Raise<T>(Expression<Func<T>> propertyExpression)
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

                PropertyChangedEventArgs e = new PropertyChangedEventArgs(body.Member.Name);
                try
                {
                    PropertyChanged(target, e);
                    OnPropertyRaised(target, e.PropertyName);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "On RAISE");
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
