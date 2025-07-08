using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Effects;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions.Validators;
internal class ProfileCardDefineActionValidator : IActionEffectValidator<ProfileCardDefineAction, ProfileCardDefineEffect>
{
    public Task<bool> Validate(ProfileCardDefineAction action)
    {
        if (action.TestData == "Error")
            return Task.FromResult(false);
        return Task.FromResult(true);
    }
}
