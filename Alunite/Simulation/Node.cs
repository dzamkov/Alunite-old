using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A reference to a particular feature within an entity that can be accessed and used externally on the entity.
    /// </summary>
    /// <remarks>Terminals are only compared by reference and therfore, need no additional information.</remarks>
    public class Node
    {
        public override string ToString()
        {
            return this.GetHashCode().ToString();
        }
    }
}