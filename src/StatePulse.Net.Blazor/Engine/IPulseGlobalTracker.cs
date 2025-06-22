namespace StatePulse.Net.Blazor.Engine;
public interface IPulseGlobalTracker
{
    public int ActivePulsars { get; }
    public void Register(IPulse pulsar);
    public void UnRegister(IPulse pulsar);
    public event EventHandler? onAfterCleanUp;
}
