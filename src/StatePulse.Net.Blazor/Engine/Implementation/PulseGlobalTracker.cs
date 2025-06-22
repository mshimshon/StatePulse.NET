
namespace StatePulse.Net.Blazor.Engine.Implementation;
internal class PulseGlobalTracker : IPulseGlobalTracker
{
    private readonly object _lock = new();
    public int ActivePulsars { get => _registry.Count; }
    private readonly List<IPulse> _registry = new();
    private IReadOnlyList<IPulse> _readRegistry
    {
        get
        {
            lock (_lock)
                return _registry.ToList();
        }
    }
    private readonly Timer _timer;

    public event EventHandler? onAfterCleanUp;

    public PulseGlobalTracker()
    {
        _timer = new Timer(GarbageCollecting, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
    }
    public void Register(IPulse pulsar)
    {
        lock (_lock)
            _registry.Add(pulsar);
    }
    public void UnRegister(IPulse pulsar)
    {
        lock (_lock)
            _registry.Remove(pulsar);
    }

    private void GarbageCollecting(object? _)
    {
        foreach (var item in _readRegistry)
            item.SelfDisposeCheck();
        onAfterCleanUp?.Invoke(this, new());
    }

}
