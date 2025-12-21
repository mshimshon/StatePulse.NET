using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.Net;

public interface IReducerSingleton<TState, in TAction> : IReducer<TState, TAction>
    where TState : IStateFeature
    where TAction : IAction
{
}
