namespace StatePulse.Net;
public interface IPulseGlobalTracker
{
    public int ActivePulsars { get; }
    public void Register(IStatePulse pulsar);
    public void UnRegister(IStatePulse pulsar);
    public event EventHandler? onAfterCleanUp;
}
