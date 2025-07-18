using StatePulse.Net.Engine.Implementations;

namespace StatePulse.Net.Engine;
public interface IDispatchHandler
{
    Task MaintainChainKey(DispatchTrackingIdentity chainKey);
}
