## StatePulse.Net v0.9.21
- Implement the Blazor Package and removed dependencies to Blazor ComponentBase which is no longer required... 
- Any objects within .NET can now use IStatePulse and benefit from state management without extra implementations.
- Renamed IPulse to IStatePulse
- using IStatePulse.StateOf<>(()=>this, () => InvokeAsync(StateHasChanged));


## Blazor Packages v0.9.2
- Deprecated now part of StatePulse regular since we have removed the dependencies to blazor component.
... that was quick!


