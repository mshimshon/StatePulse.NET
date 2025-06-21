using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Store;
public record ProfileCardState : IStateFeature
{
    public string? ProfileName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? ProfileId { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
