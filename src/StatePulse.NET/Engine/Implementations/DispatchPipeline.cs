namespace StatePulse.Net.Engine.Implementations;

internal class DispatchPipeline : IDispatchPipeline
{

    public DispatchPipeline(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
