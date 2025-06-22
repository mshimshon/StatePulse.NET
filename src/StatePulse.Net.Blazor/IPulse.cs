namespace StatePulse.Net.Blazor;
public interface IPulse : IDisposable
{
    TState StateOf<TState>(object instance) where TState : IStateFeature;
    bool IsReferenceAlive();
    void SelfDisposeCheck();

}
