namespace StatePulse.Net;
public interface IPulseGlobalTracker
{
    int ActivePulsars { get; }
    void Register(IStatePulse pulsar);
    void UnRegister(IStatePulse pulsar);
    IReadOnlyCollection<IStatePulse> Registered { get; }
    event EventHandler? onAfterCleanUp;
}
