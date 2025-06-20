namespace StatePulse.Net.Validation;
public class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public bool IsValid => _errors.Count == 0;

    public IReadOnlyList<ValidationError> Errors => _errors;

    public void AddError(string errorCode, string errorMessage)
    {
        _errors.Add(new ValidationError(errorCode, errorMessage));
    }
}
