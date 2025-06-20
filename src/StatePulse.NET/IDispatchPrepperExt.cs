using StatePulse.Net.Internal;
using StatePulse.Net.Internal.Extensions;
using System.Linq.Expressions;

namespace StatePulse.Net;
public static class IDispatchPrepperExt
{
    /// <summary>
    /// Set Property chain-expression style.
    /// </summary>
    public static IDispatcherPrepper<TAction> With<TAction, TValue>(this IDispatcherPrepper<TAction> prep, Expression<Func<TAction, TValue>> expression, TValue value)
    {
        var instanceType = typeof(TAction)!;
        var propertyName = expression.GetPropetyName();
        var prop = instanceType.GetProperty(propertyName)!;
        prop.SetValue(prep.ActionInstance, value);
        return prep;
    }

}
