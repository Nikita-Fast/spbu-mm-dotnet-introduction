using System.Linq.Expressions;


namespace ReflectionLib
{
    public static class ReflectionHelper

    {
        public static Func<T, U> MakeGetter<T, U>(string path)
        {
            var parameter = Expression.Parameter(typeof(T));
            Expression body = parameter;

            var memberNames = path.Split('.');

            for (int i = 0; i < memberNames.Length; i++)
            {
                var memberName = memberNames[i];

                if (memberName.Contains("["))
                {
                    int pos = memberName.IndexOf("[");
                    string arrayPropertyName = memberName.Substring(0, pos);
                    var indexes_str = memberName.Substring(pos, memberName.Length - pos);
                    var indexes = indexes_str.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    body = Expression.PropertyOrField(body, arrayPropertyName);
                    foreach (var idx in indexes)
                    {
                        int arrayIndex = int.Parse(idx);

                        if (body.Type.IsArray)
                        {
                            body = Expression.ArrayIndex(body, Expression.Constant(arrayIndex));
                        }
                        else if (body.Type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)))
                        {
                            var indexer = body.Type.GetProperty("Item");
                            var indexValue = Expression.Constant(arrayIndex);
                            body = Expression.MakeIndex(body, indexer, new[] { indexValue });
                        }
                        else
                        {
                            body = Expression.PropertyOrField(body, arrayPropertyName);
                        }
                    }
                }
                else
                {
                    body = Expression.PropertyOrField(body, memberName);
                }
            }

            var lambda = Expression.Lambda<Func<T, U>>(body, parameter);
            return lambda.Compile();
        }
    }
}
