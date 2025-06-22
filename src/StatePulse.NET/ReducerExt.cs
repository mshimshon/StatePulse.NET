using System.Linq.Expressions;
using System.Reflection;

namespace StatePulse.Net;
public static class ReducerExt
{
    public static Cloner<T> ReducerResult<T>(T state)
        where T : class, new()
        => new(state);

    public class Cloner<T> where T : class, new()
    {
        private readonly T _source;
        private readonly Dictionary<string, object?> _overrides = new();

        public Cloner(T source)
        {
            _source = source;
        }

        public Cloner<T> With<TProp>(Expression<Func<T, TProp>> selector, TProp value)
        {
            if (selector.Body is not MemberExpression member)
                throw new ArgumentException("Expression must be a property access.", nameof(selector));

            _overrides[member.Member.Name] = value;
            return this;
        }

        public Task<T> ToTask()
        {
            var result = CloneWithOverrides(_source, _overrides);
            return Task.FromResult(result);
        }

        private static T CloneWithOverrides(T source, Dictionary<string, object?> overrides)
        {
            var type = typeof(T);
            var clone = Activator.CreateInstance<T>();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite)
                    continue;

                var value = overrides.TryGetValue(prop.Name, out var overrideValue)
                    ? overrideValue
                    : prop.GetValue(source);

                prop.SetValue(clone, value);
            }

            return clone;
        }
    }

}
