using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Shared.Contracts.Response;
namespace StatePulse.NET.Tests.TestCases.Pulsars.Profile.Effects;

internal class ProfileCardDefineEffect : IEffect<ProfileCardDefineAction>
{
    private readonly IStateAccessor<MainMenuState> _stateAccessor;

    public ProfileCardDefineEffect(IStateAccessor<MainMenuState> stateAccessor)
    {
        _stateAccessor = stateAccessor;
    }
    public async Task EffectAsync(ProfileCardDefineAction action, IDispatcher dispatcher)
    {
        await dispatcher.Prepare<ProfileCardLoaderStartAction>().DispatchAsync();

        await Task.Delay(action.Delay);
        var myProfile = new UserResponse();
        await dispatcher.Prepare(() => new ProfileCardDefineResultAction(action.TestData ?? myProfile.Name, myProfile.Picture, myProfile.Id) { UnitTestStringer = action.TestData }).DispatchAsync();
        await dispatcher.Prepare<ProfileCardLoaderStopAction>().DispatchAsync();
    }

}
