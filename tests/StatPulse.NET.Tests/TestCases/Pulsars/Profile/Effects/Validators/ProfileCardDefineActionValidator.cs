using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Effects.Validators;
/*
* This is the best way to define clean conditional effects, it either run or not... this is not meant for triggering errors.
* This is meant for optional/condition effects to either run or not base on the action settings...
*/
internal class ProfileCardDefineActionValidator : IEffectValidator<ProfileCardDefineAction, ProfileCardDefineEffect>
{
    public Task<bool> Validate(ProfileCardDefineAction action)
    {
        if (action.TestData == "Error")
            return Task.FromResult(false);
        return Task.FromResult(true);
    }
}
