namespace StatePulse.Net.Engine.Implementations;

using StatePulse.Net;


internal class PulseGlobalTracker : IPulseGlobalTracker
{
    private delegate void SelfCheckRequired();
    private readonly object _lock = new();
    /// <summary>
    /// TODO: V3.0 Change to _notifyForSelfCheck?.GetInvocationList().Length ?? 0
    /// </summary>
    public int ActivePulsars { get => _registry.Count; }
    /// <summary>
    /// TODO: REMOVE AT 3.0
    /// </summary>
    private readonly List<IStatePulse> _registry = new();
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
    private event SelfCheckRequired? _notifyForSelfCheck;
    private bool _pendingCheck;
    private bool _isLoopRunning;

    private IReadOnlyList<IStatePulse> _readRegistry
    {
        get
        {
            lock (_lock)
                return _registry.ToList();
        }
    }

    public IReadOnlyCollection<IStatePulse> Registered => _registry;


    public event EventHandler? onAfterCleanUp;

    public PulseGlobalTracker()
    {
        _ = Task.Run(async () => await GarbageCollecting());
    }
    public void Register(IStatePulse pulsar)
    {
        lock (_lock)
        {
            _notifyForSelfCheck += pulsar.SelfDisposeCheck;
            _registry.Add(pulsar);
            if (_isLoopRunning)
                _pendingCheck = true;
            else
                _signal.Release();
        }


    }
    public void UnRegister(IStatePulse pulsar)
    {
        lock (_lock)
        {
            _notifyForSelfCheck -= pulsar.SelfDisposeCheck;
            _registry.Remove(pulsar);
            if (_isLoopRunning)
                _pendingCheck = true;
            else
                _signal.Release();
        }
    }

    private async Task GarbageCollecting()
    {
        do
        {
            if (!_pendingCheck)
                await _signal.WaitAsync();
            if (_isLoopRunning)
                continue; // Already running → coalesce
            lock (_lock)
                _pendingCheck = false;

            _isLoopRunning = true;
            _notifyForSelfCheck?.Invoke();
            onAfterCleanUp?.Invoke(this, new());
            // Enforce a minimum delay before checking again
            await Task.Delay(TimeSpan.FromSeconds(10));
            _isLoopRunning = false;
        } while (true);

    }

}
