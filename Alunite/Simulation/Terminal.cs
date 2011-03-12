using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A reference to a bidirectional channel in an entity that can be used for communication with complimentary terminals over time.
    /// </summary>
    /// <typeparam name="TInput">The type of input received on the channel at any one time.</typeparam>
    /// <typeparam name="TOutput">The type of output given by the channel at any one time.</typeparam>
    public class Terminal<TInput, TOutput> : Node
    {
        
    }

    /// <summary>
    /// A terminal that receives input without giving any output.
    /// </summary>
    public class InTerminal<TInput> : Terminal<TInput, Void>
    {

    }

    /// <summary>
    /// A terminal that gives output without receiving any input.
    /// </summary>
    public class OutTerminal<TOutput> : Terminal<Void, TOutput>
    {

    }
}