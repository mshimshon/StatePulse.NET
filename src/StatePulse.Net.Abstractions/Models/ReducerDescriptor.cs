namespace StatePulse.Net.Models;

public sealed record ReducerDescriptor
{
    public Type StateType { get; init; } = default!;
    public Type ActionType { get; init; } = default!;
    public Type ServiceType { get; init; } = default!;
}
