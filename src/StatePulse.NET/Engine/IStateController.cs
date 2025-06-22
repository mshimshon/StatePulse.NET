namespace StatePulse.Net.Engine;
internal interface IStateController<TState>
{
    public TState State { get; set; }
}
