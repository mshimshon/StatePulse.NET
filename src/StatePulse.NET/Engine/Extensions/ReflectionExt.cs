using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace StatePulse.Net.Engine.Extensions;
internal static class ReflectionExt
{
    public static Action<object, object?> CreateSetterDynamic(this PropertyInfo property)
    {
        if (!property.CanWrite)
            throw new ArgumentException("Property does not have a setter.");

        var declaringType = property.DeclaringType ?? throw new ArgumentException("Property must have declaring type");

        var method = new DynamicMethod(
            name: $"__set_{declaringType.Name}_{property.Name}_{Guid.NewGuid()}",
            returnType: typeof(void),
            parameterTypes: new[] { typeof(object), typeof(object) },
            m: declaringType.Module,
            skipVisibility: true);

        var il = method.GetILGenerator();

        // Load target (arg0) and cast/unbox to declaring type
        il.Emit(OpCodes.Ldarg_0);
        il.EmitUnboxOrCast(declaringType);

        // Load value (arg1) and unbox/cast to property type
        il.Emit(OpCodes.Ldarg_1);
        il.EmitUnboxOrCast(property.PropertyType);

        // Call set_Property
        il.EmitCall(OpCodes.Callvirt, property.SetMethod!, null);

        il.Emit(OpCodes.Ret);

        return (Action<object, object?>)method.CreateDelegate(typeof(Action<object, object?>));
    }
    public static Func<object, object?> CreateGetterDynamic(this PropertyInfo property)
    {
        var declaringType = property.DeclaringType ?? throw new ArgumentException("Property must have declaring type");

        var method = new DynamicMethod(
            name: $"__get_{declaringType.Name}_{Guid.NewGuid()}",
            returnType: typeof(object),
            parameterTypes: new[] { typeof(object) },
            m: declaringType.Module,
            skipVisibility: true);

        var il = method.GetILGenerator();

        // Load object arg0 -> cast to declaring type
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, declaringType);
        il.EmitUnboxOrCast(declaringType);
        // Call get_State
        il.EmitCall(OpCodes.Callvirt, property.GetMethod!, null);
        il.EmitBoxIfNeeded(property.PropertyType);


        il.Emit(OpCodes.Ret);

        return (Func<object, object?>)method.CreateDelegate(typeof(Func<object, object?>));
    }


    public static Func<object, object?[]?, object?> CreateDynamicReflectionInvoker(this MethodInfo method)
    {
        if (method == null)
            throw new ArgumentNullException(nameof(method));

        var declaringType = method.DeclaringType ?? throw new ArgumentException("Method must have declaring type");

        var dynamicMethod = new DynamicMethod(
            $"__dyn_{declaringType.Name}_{method.Name}",
            typeof(object),                              // return type
            new[] { typeof(object), typeof(object?[]) }, // parameters: target, args[]
            declaringType.Module,
            skipVisibility: true);

        var il = dynamicMethod.GetILGenerator();

        var parameters = method.GetParameters();

        // Load target instance if not static
        if (!method.IsStatic)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, declaringType);
        }

        // Load each argument from object[] and cast to appropriate type
        for (int i = 0; i < parameters.Length; i++)
        {
            il.Emit(OpCodes.Ldarg_1);                       // Load args[]
            il.Emit(OpCodes.Ldc_I4, i);                     // Index
            il.Emit(OpCodes.Ldelem_Ref);                    // args[i]
            EmitUnboxOrCast(il, parameters[i].ParameterType); // Cast to param type
        }

        // Call method
        il.EmitCall(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method, null);

        // Handle return
        if (method.ReturnType == typeof(void))
        {
            il.Emit(OpCodes.Ldnull);
        }
        else if (method.ReturnType.IsValueType)
        {
            il.Emit(OpCodes.Box, method.ReturnType);
        }

        il.Emit(OpCodes.Ret);

        return (Func<object, object?[]?, object?>)dynamicMethod.CreateDelegate(typeof(Func<object, object?[]?, object?>));
    }
    private static void EmitUnboxOrCast(this ILGenerator il, Type type)
    {
        if (type.IsValueType)
            il.Emit(OpCodes.Unbox_Any, type);
        else
            il.Emit(OpCodes.Castclass, type);
    }

    private static void EmitBoxIfNeeded(this ILGenerator il, Type type)
    {
        if (type.IsValueType)
            il.Emit(OpCodes.Box, type);
    }
    public static Func<object, Task> CreateDynamicInvoker(this MethodInfo method)
    {
        var declaringType = method.DeclaringType ?? throw new ArgumentException("Method must have a declaring type");

        var dm = new DynamicMethod(
            $"__dyn_{method.Name}_{Guid.NewGuid()}",
            typeof(Task),
            new[] { typeof(object) },
            declaringType.Module,
            skipVisibility: true);

        var il = dm.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.EmitUnboxOrCast(declaringType);
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