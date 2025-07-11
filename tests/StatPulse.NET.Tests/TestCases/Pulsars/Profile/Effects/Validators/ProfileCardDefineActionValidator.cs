using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Effects.Validators;
internal class ProfileCardDefineActionValidator : IEffectValidator<ProfileCardDefineAction, ProfileCardDefineEffect>
{
    public Task<bool> Validate(ProfileCardDefineAction action)
    {
        if (action.TestData == "Error")
            return Task.FromResult(false);
        return Task.FromResult(true);
    }
}
