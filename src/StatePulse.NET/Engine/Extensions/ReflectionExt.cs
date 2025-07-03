using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace StatePulse.Net.Engine.Extensions;
internal static class ReflectionExt
{

    public static Func<object, Task> CreateDynamicInvoker(this MethodInfo method)
    {
        var declaringType = method.DeclaringType ?? throw new ArgumentException("Method must have a declaring type");

        var dm = new DynamicMethod(
            $"__dyn_{method.Name}",
            typeof(Task),
            new[] { typeof(object) },
            declaringType.Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, declaringType);

        il.EmitCall(OpCodes.Call, method, null);

        il.Emit(OpCodes.Ret);

        return (Func<object, Task>)dm.CreateDelegate(typeof(Func<object, Task>));
    }
    public static MethodInfo GetMethodInfoOrThrow(this Func<Task> func)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));

        MethodInfo method = func.Method;

        // Check for compiler-generated attribute, indicating lambda or anonymous method
        bool isCompilerGenerated = method.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true);

        if (isCompilerGenerated)
            throw new InvalidOperationException("Lambdas and anonymous methods are not allowed. Please pass a method group (e.g., instance.MethodName).");

        return method!;
    }
    public static KeyValuePair<Expression<Func<TEntity, TValue>>, TValue> PropSetToKeyPair<TEntity, TValue>(Expression<Func<TEntity, TValue>> property, TValue value)
    {
        var propname = property.GetPropetyName();
        return new KeyValuePair<Expression<Func<TEntity, TValue>>, TValue>(property, value);
    }

    public static string GetPropetyName<TValue>(this Expression<Func<TValue>> property)
        => property.GetPropetyInfo().Name;

    public static string GetPropetyName<TEntity, TValue>(this Expression<Func<TEntity, TValue>> property)
    => property.GetPropetyInfo().Name;

    public static PropertyInfo GetPropetyInfo<TEntity, TValue>(this Expression<Func<TEntity, TValue>> property)
    {
        var memberExpression = property.Body is UnaryExpression expression
            ? (MemberExpression)expression.Operand
            : (MemberExpression)property.Body;
        return (PropertyInfo)memberExpression.Member;
    }

    public static PropertyInfo GetPropetyInfo<TValue>(this Expression<Func<TValue>> property)
    {
        var memberExpression = property.Body is UnaryExpression expression
            ? (MemberExpression)expression.Operand
            : (MemberExpression)property.Body;
        return (PropertyInfo)memberExpression.Member;
    }
}