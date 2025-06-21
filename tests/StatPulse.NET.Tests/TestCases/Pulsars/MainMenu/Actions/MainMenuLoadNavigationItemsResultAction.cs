using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
public record MainMenuLoadNavigationItemsResultAction(List<string> Items) : IAction
{
}
