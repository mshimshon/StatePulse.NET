namespace StatePulse.Net.Engine.Implementations;
public record DispatchTrackingIdentity
{
    public Guid Id { get; init; }
    public Type EntryType { get; init; } = default!;
}
