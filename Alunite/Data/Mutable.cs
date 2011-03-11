using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A mutable object that informs subscribers when it is changed.
    /// </summary>
    /// <typeparam name="TDesc">A description of a change.</typeparam>
    public interface IMutable<TDesc>
    {
        /// <summary>
        /// An event fired when the mutable object's outward interface is modified.
        /// </summary>
        event ChangedHandler<TDesc> Changed;
    }

    /// <summary>
    /// An event handler for a change in a mutable object.
    /// </summary>
    public delegate void ChangedHandler<TDesc>(TDesc ChangeDescription);
}