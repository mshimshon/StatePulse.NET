namespace StatePulse.Net;
public interface IStatePulse : IDisposable
{
    TState StateOf<TState>(object instance) where TState : IStateFeature;

    TState StateOf<TState>(object instance, Func<Task> onStateChanged) where TState : IStateFeature;
    Task InitializeListennerAsync(object instance, Func<Task> onStateChanged);
    bool IsReferenceAlive();
    void SelfDisposeCheck();

}
