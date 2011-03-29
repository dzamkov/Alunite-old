using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity which transforms a phantom entity in response to a signal on a terminal.
    /// </summary>
    public class MoverEntity : PhantomEntity
    {
        public MoverEntity(Entity Source)
        {
            this._Source = Source;
            this._Terminal = new InTerminal<Transform>();
        }

        /// <summary>
        /// Gets the entity that is displaced. This should be a phantom entity.
        /// </summary>
        public Entity Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the input terminal that determines the transform the mover applies.
        /// </summary>
        public InTerminal<Transform> Input
        {
            get
            {
                return this._Terminal;
            }
        }

        private Entity _Source;
        private InTerminal<Transform> _Terminal;
    }
}