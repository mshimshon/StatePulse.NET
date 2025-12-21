using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.Net.Engine.Exceptions;

public class InvalidDispatchCombinationException : Exception
{
    public InvalidDispatchCombinationException(string? message) : base(message)
    {
    }
}
