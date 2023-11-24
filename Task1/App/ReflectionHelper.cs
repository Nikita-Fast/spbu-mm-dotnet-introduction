using System.Linq.Expressions;

namespace NReflectionHelper
{
    public static class ReflectionHelper
    {
        public static Func<T, U> MakeGetter<T, U>(string path)
        {
            var parameter = Expression.Parameter(typeof(T), "obj");
            Expression propertyExpression = parameter;

            foreach (var propertyName in path.Split('.'))
            {
                propertyExpression = Expression.PropertyOrField(propertyExpression, propertyName);
            }

            var lambdaExpression = Expression.Lambda<Func<T, U>>(propertyExpression, parameter);
            return lambdaExpression.Compile();
        }
    }
}
