
<<<<<<< TODO: Unmerged change from project 'StatePulse.Net.Abstractions (net8.0)', Before:
namespace StatePulse.Net.Abstractions;
public interface IDispatcherPrepper<TAction> where TAction : IAction
=======
using StatePulse;
using StatePulse.Net;
using StatePulse.Net;
using StatePulse.Net.Abstractions;

namespace StatePulse.Net;
public interface IDispatcherPrepper<TAction> where TAction : IAction
>>>>>>> After
using StatePulse;
using StatePulse.Net;
using StatePulse.Net.Abstractions;

namespace StatePulse.Net;
public interface IDispatcherPrepper<TAction> where TAction : IAction
{
    TAction ActionInstance { get; }
    IDispatcherPrepper<TAction> Await();
    Task<Guid> DispatchAsync(bool asSafe = false);

}
