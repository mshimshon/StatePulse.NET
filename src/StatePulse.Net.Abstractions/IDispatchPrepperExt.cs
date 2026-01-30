using System.Linq.Expressions;
using System.Reflection;

namespace StatePulse.Net;

public static class IDispatchPrepperExt
{
    public static IDispatcherPrepper<TAction> With<TAction, TValue>(this IDispatcherPrepper<TAction> prep, Expression<Func<TAction, TValue>> expression, TValue value)
        where TAction : IAction
    {
        var instanceType = typeof(TAction)!;
        var propertyName = expression.GetPropetyName();
        var prop = instanceType.GetProperty(propertyName)!;
        prop.SetValue(prep.ActionInstance, value);
        return prep;
    }

    private static string GetPropetyName<TEntity, TValue>(this Expression<Func<TEntity, TValue>> property)
    => property.GetPropetyInfo().Name;

    private static PropertyInfo GetPropetyInfo<TEntity, TValue>(this Expression<Func<TEntity, TValue>> property)
    {
        var memberExpression = property.Body is UnaryExpression expression
            ? (MemberExpression)expression.Operand
            : (MemberExpression)property.Body;
        return (PropertyInfo)memberExpression.Member;
    }

}
