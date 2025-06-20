namespace StatePulse.Net.Internal.Implementations;
internal record DispatchCancelToken()
{
    private bool _isDispose = false;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public bool IsDispose => _isDispose;
    public bool IsCancellationRequested { get => _cancellationTokenSource.IsCancellationRequested; }
    public void Cancel()
    {
        if (IsDispose) return;
        _cancellationTokenSource.Cancel();
    }
    public void Destroy()
    {
        if (IsDispose) return;
        _cancellationTokenSource.Dispose();
        _isDispose = true;
    }
}
