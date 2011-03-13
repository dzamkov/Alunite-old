using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A reference to a terminal which takes an input signal produced by a complimentary terminal.
    /// </summary>
    public class InTerminal<T> : Node
    {

    }

    /// <summary>
    /// A reference to a terminal which produces an output signal that may be used by a complimentary terminal.
    /// </summary>
    public class OutTerminal<T> : Node
    {

    }
}