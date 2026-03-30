using StatePulse.Net.Engine.Implementations;

namespace StatePulse.Net.Engine;

public interface IDispatchHandler
{
    void MaintainChainKey(DispatchTrackingIdentity chainKey);
    void AssignToken(CancellationToken ct);
    void NextAwaited();

}
