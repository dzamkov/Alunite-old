using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An immutable representation of an object. Note that methods calls on data may have inaccuracies and error, but the data must still
    /// be self-consistent and unambiguously represent an object.
    /// </summary>
    public abstract class Data<TBase>
        where TBase : Data<TBase>
    {
        /// <summary>
        /// Gets a simplified form of this object. The simplified data represents the same object,
        /// but is usually smaller and faster performace-wise. Simple simplifications can be made in the constructors
        /// of data. Since this does not change the meaning of data, it may be performed in place, and change the
        /// data.
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