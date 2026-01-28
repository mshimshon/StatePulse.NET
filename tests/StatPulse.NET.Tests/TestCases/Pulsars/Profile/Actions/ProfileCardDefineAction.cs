using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;

public record ProfileCardDefineAction : IAction
{
    public string? TestData { get; set; }
    public int Delay { get; set; } = 100;
}
