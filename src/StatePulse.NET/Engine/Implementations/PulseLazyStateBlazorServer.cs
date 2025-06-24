using System.Collections.Concurrent;

namespace StatePulse.Net.Engine;
internal sealed class PulseLazyStateBlazorServer : PulseLazyStateBase
{
    private readonly ConcurrentDictionary<Type, IStateAccessor<object>> _stash = new();
    public PulseLazyStateBlazorServer(IServiceProvider services) : base(services)
    {
    }
    protected override IDictionary<Type, IStateAccessor<object>> GetState() => _stash;
}
