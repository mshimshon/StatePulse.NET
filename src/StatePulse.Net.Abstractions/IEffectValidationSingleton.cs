using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.Net;

internal interface IEffectValidationSingleton<in TAction, TEffect> : IEffectValidator<TAction, TEffect> 
    where TAction : IAction
    where TEffect : IEffect<TAction>
{
}
