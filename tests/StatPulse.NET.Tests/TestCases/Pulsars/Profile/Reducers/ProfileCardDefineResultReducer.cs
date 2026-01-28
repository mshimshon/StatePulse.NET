using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Reducers;

internal class ProfileCardDefineResultReducer : IReducer<ProfileCardState, ProfileCardDefineResultAction>
{
    public ProfileCardState Reduce(ProfileCardState state, ProfileCardDefineResultAction action)
        => state with
        {
            LastUpdate = DateTime.UtcNow,
            ProfileId = action.Id,
            ProfileName = action.Name,
            ProfilePicture = action.Picture,
            UnitTestStringer = action.UnitTestStringer
        };
}
