namespace StatePulse.Net.Configuration;

public enum DispatchEffectExecutionBehavior
{
    /// <summary>
    /// (Default): Like fire and forget but the statepulse engine still awaits all the effects before calling the middlewares.
    /// </summary>
    YieldAndFire,
}
