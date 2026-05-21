using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

public sealed record MainMenuSecondState : IStateFeature
{
    public bool IsSuccessful { get; set; }
}
