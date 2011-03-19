using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An immutable representation of an object.
    /// </summary>
    public abstract class Data<TBase>
        where TBase : Data<TBase>
    {
        /// <summary>
        /// Gets a simplified form of this object. The simplified data represents the same object,
        /// but is usually smaller and faster performace-wise. Simple simplifications can be made in the constructors
        /// of data.
        /// </summary>
        public virtual TBase Simplify
        {
            get
            {
                return (TBase)this;
            }
        }
    }
}