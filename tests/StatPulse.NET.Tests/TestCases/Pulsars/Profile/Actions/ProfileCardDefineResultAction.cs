using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;
public record ProfileCardDefineResultAction(string Name, string Picture, string Id) : IAction
{
    public string? UnitTestStringer { get; init; }
}
