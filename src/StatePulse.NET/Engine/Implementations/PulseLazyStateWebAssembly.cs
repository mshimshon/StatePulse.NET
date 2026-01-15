namespace StatePulse.Net.Engine.Implementations;

using System;
internal sealed class PulseLazyStateWebAssembly : PulseLazyStateBase
{
    private readonly Dictionary<Type, object> _stash = new();

    public PulseLazyStateWebAssembly(IServiceProvider services) : base(services)
    {
    }

    protected override IDictionary<Type, object> GetState() => _stash;
}