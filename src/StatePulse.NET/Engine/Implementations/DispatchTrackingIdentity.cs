namespace StatePulse.Net.Engine.Implementations;

public record DispatchTrackingIdentity
{
    public IDispatchPipeline Pipeline { get; init; } = default!;
    public Type EntryType { get; init; } = default!;
    public IDispatchEntry TrackedEntry { get; set; } = default!;
    public long Version { get; init; }
    public Func<IDispatchTracker> Tracker { get; init; } = default!;
}
