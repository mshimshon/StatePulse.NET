## 🔖 Versioning Policy

### 🚧 Pre-1.0.0 (`0.x.x`)

- The project is considered **Work In Progress**.
- **Breaking changes can occur at any time** without notice.
- No guarantees are made about stability or upgrade paths.

### ✅ Post-1.0.0 (`1.x.x` and beyond)

Follows a common-sense semantic versioning pattern:

- **Major (`X.0.0`)**  
  
  - Introduces major features or architectural changes  
  - May include well documented **breaking changes**

- **Minor (`1.X.0`)**  
  
  - Adds new features or enhancements  
  - May include significant bug fixes  
  - **No breaking changes**

- **Patch (`1.0.X`)**  
  
  - Hotfixes or urgent bug fixes  
  - Safe to upgrade  
  - **No breaking changes**
  

## v1.0.0
### New Features
- Added Action Effect Validator, which validate an action effect before it runs to allow conditional effect runners.
- Compiler Error Generator when using Lambda StateOf(()=> this, ()=> XX) which prevent runtime exceptions.
- Added Middleware Support ```IEffectMiddleware```, ```IReducerMiddleware``` and ```IDispatchMiddleware```.
- Some Decision i couldn't take for you so you have behavior options at service registration ```DispatchEffectBehavior```, ```MiddlewareEffectBehavior``` & ```MiddlewareTaskBehavior```
### Breaking Changes
- Remove Action Validator, which validated an action itself where it is not the role of State Management to validate actions.
- Rename ```IStateAccessor<>.StateChanged``` to ```OnStateChanged```.
- Removed ```UsingSynchronousMode``` and Renamed ```Sync``` to ```Await``` for accuracy.
### Performance
- Added Extra Caching for Dispatcher.
- Added Better Type Cache in StatePulseRegistry.
### Clean Code
- Factored DispatchPrepper for cleaner and ligther methods.
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


