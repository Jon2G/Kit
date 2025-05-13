using System.Linq.Expressions;
using System.Reflection;

namespace Kit
{
    public static class TypeExtensions
    {
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static bool IsOverriden(this PropertyInfo property)
        {
            var getMethod = property.GetGetMethod(false);
            if (getMethod.GetBaseDefinition().DeclaringType == getMethod.DeclaringType)
            {
                return false;
            }
            return true;
        }

        public static bool IsPrimitive(this Type t)
        {
            return (t.IsPrimitive || t == typeof(Decimal) || t == typeof(String) || t.IsEnum || t == typeof(DateTime) ||
                    t == typeof(TimeSpan) || t == typeof(DateTimeOffset));
        }

        public static PropertyInfo GetPropertyInfo<T, TValue>(Expression<Func<T, TValue>> selector)
        {
            Expression body = selector;
            if (body is LambdaExpression)
            {
                body = ((LambdaExpression)body).Body;
            }
            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (PropertyInfo)((MemberExpression)body).Member;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}