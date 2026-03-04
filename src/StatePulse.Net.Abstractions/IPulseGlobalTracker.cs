namespace StatePulse.Net;

public interface IPulseGlobalTracker
{
    int ActivePulsars { get; }
    void Register(IStatePulse pulsar);
    void UnRegister(IStatePulse pulsar);
    [Obsolete("Will be removed in in V3.0 Do not use or rely on this")]
    IReadOnlyCollection<IStatePulse> Registered { get; }
    event EventHandler? onAfterCleanUp;
}
