namespace StatePulse.Net;
public interface IStatePulse : IDisposable
{
    TState StateOf<TState>(Func<object> getInstance, Func<Task> onStateChanged) where TState : IStateFeature;
    bool IsReferenceAlive();
    void SelfDisposeCheck();

}
