## v1.0.0
### New Features
- Added Action Effect Validator, which validate an action effect before it runs to allow conditional effect runners.
### Breaking Changes
- Remove Action Validator, which validated an action itself where it is not the role of State Management to validate actions.
### Performance
- Added Extra Caching for Dispatcher.
- Added Better Type Cache in StatePulseRegistry.
### Fixes
- Fix Several Null-Reference Warnings
- Remove Several Artifacts.

## v0.9.41
- Fix: Added Anti-Service duplication to avoid double triggers.

## v0.9.4
- Breaking Change, StateOf no longer accept lambda will throw exception you must define a Task directly... this was necessary due to Garbage Collector and tracking behavior.
- Deprecated UsingSynchronousMode() instead use Sync().

## v0.9.21
- Implement the Blazor Package and removed dependencies to Blazor ComponentBase which is no longer required... 
- Any objects within .NET can now use IStatePulse and benefit from state management without extra implementations.
- Renamed IPulse to IStatePulse
- using IStatePulse.StateOf<>(()=>this, () => InvokeAsync(StateHasChanged));


## v0.9.2 (Blazor Packages)
- Deprecated now part of StatePulse regular since we have removed the dependencies to blazor component.
... that was quick!


