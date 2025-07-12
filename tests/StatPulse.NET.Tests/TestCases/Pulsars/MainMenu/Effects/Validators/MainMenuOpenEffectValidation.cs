using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Effects.Validators;
internal class MainMenuOpenEffectValidation : IEffectValidator<MainMenuOpenAction, MainMenuOpenEffect>
{
    public Task<bool> Validate(MainMenuOpenAction action) => Task.FromResult(true);
}
