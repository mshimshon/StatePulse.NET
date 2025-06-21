using StatePulse.Net;
using StatePulse.Net.Validation;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions.Validators;
internal class ProfileCardDefineActionValidator : IActionValidator<ProfileCardDefineAction>
{
    public void Validate(ProfileCardDefineAction action, ref ValidationResult result)
    {
        if (action.TestData == "Error")
            result.AddError("ErrorName", "Name Cannot be Error");
    }
}
