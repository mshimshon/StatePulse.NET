namespace StatePulse.Net.Engine;

internal interface IStateCallbackBinder : IDisposable
{
    Type StateType { get; }
    object GetState();
    T GetStateAs<T>();
    Task TriggerCallback();
}
internal sealed record StateCallbackBinder<TState> : IStateCallbackBinder
{
    public Func<object, Task> Callback { get; init; } = default!;
    public Type StateType { get; }
    public Func<WeakReference<object?>> CheckIfInstanceAlive = default!;
    private bool _disposedValue;

    public IStateAccessor<TState> StateAccess { get; }

    public StateCallbackBinder(IStateAccessor<TState> stateAccessor)
    {
        StateType = typeof(TState);
        StateAccess = stateAccessor;
        StateAccess.OnStateChangedNoDetails += OnStateChanged;
    }
    public bool Equals(StateCallbackBinder<TState>? other)
    {
        if (other is null)
            return false;

        // Equality based ONLY on the Type
        return StateType == other.StateType;
    }

    public override int GetHashCode()
    {
        // Hash based ONLY on the Type
        return StateType.GetHashCode();
    }

    public object GetState() => StateAccess.State!;
    public T GetStateAs<T>() => (T)GetState();
    public Task TriggerCallback() => throw new NotImplementedException();
    public void OnStateChanged(object? sender, EventArgs args)
    {
        var instance = CheckIfInstanceAlive?.Invoke() ?? default;
        if (instance != default && instance.TryGetTarget(out var target))
        {
            _ = Callback(target);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                StateAccess.OnStateChangedNoDetails -= OnStateChanged;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
