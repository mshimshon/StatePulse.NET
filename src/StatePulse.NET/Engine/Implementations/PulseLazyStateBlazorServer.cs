using System.Collections.Concurrent;
namespace StatePulse.Net.Engine.Implementations;

internal sealed class PulseLazyStateBlazorServer : PulseLazyStateBase
{
    private readonly ConcurrentDictionary<Type, object> _stash = new();
    public PulseLazyStateBlazorServer(IServiceProvider services) : base(services)
    {
    }
    protected override IDictionary<Type, object> GetState() => _stash;
}
