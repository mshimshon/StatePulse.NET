## StatePulse.Net v0.9.3
- Implement the Blazor Package and removed dependencies to Blazor ComponentBase which is no longer required... 
- Any objects within .NET can now use IPulse and benefit from state management without extra implementations.
- Added InitializeAsync where you can called in oninitialize blazor to define InvokeAsync(()=>StateHasChanged).
- Added a Lazy Alternative where you skip the boilerplate and call StateOf<>() where you can also define the InvokeAsync.
- The InitializeAsync and StateOf<>() are usable by any object to start listening and stop listening when caller dies.


## Blazor Packages v0.9.2
- Deprecated now part of StatePulse regular since we have removed the dependencies to blazor component.
... that was quick!


