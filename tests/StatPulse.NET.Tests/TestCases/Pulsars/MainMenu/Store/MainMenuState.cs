using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;
public record MainMenuState : IStateFeature
{
    public bool IsLoading { get; set; }
    public bool IsOpened { get; set; } = false;
    public List<string>? NavigationItems { get; set; }
}
