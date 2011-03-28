using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An extended constructor for an object of a certain type.
    /// </summary>
    public abstract class Builder<TObject>
    {
        /// <summary>
        /// Creates the object described to the builder. This may only be called once per builder.
        /// </summary>
        public abstract TObject Finish();

        /// <summary>
        /// An alternative to calling "Finish()" that gives the resulting object as an out parameter for aesthetic purposes.
        /// </summary>
        public void Finish(out TObject Object)
        {
            Object = this.Finish();
        }
    }
}