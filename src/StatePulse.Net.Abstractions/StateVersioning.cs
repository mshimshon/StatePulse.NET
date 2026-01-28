namespace StatePulse.Net;

public sealed record StateVersioning
{
    public StateVersioning(Type originType, long version, Guid dispatchWriter)
    {
        OriginType = originType;
        Version = version;
        DispatchWriter = dispatchWriter;
        LastChange = DateTime.UtcNow;
    }
    public DateTime LastChange { get; }
    public Type OriginType { get; }
    public long Version { get; }
    public Guid DispatchWriter { get; }
}
