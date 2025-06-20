using StatePulse.Net.Validation;

namespace StatePulse.Net;
/// <summary>
/// Use to define validation behavior for an action... <br/>
/// some business logic are required under certain rules and those rules can be defined here! if the validation fails the action is not dispatched
/// </summary>
/// <typeparam name="TAction"></typeparam>
public interface IActionValidator<in TAction> where TAction : IAction
{
    ValidationResult Validate(TAction action);
}
