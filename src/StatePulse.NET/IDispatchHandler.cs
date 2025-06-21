using StatePulse.Net.Engine.Implementations;

namespace StatePulse.Net;
public interface IDispatchHandler
{
    Task MaintainChainKey(DispatchTrackingIdentity chainKey);
}
