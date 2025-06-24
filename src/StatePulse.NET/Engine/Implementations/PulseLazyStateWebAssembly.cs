namespace StatePulse.Net.Engine;
using System;

internal sealed class PulseLazyStateWebAssembly : PulseLazyStateBase
{
    private readonly Dictionary<Type, IStateAccessor<object>> _stash = new();

    public PulseLazyStateWebAssembly(IServiceProvider services) : base(services)
    {
    }

    protected override IDictionary<Type, IStateAccessor<object>> GetState() => _stash;
}