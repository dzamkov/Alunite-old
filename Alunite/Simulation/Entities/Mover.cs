using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity which transforms a phantom entity in response to a signal on a terminal.
    /// </summary>
    public class MoverEntity : PhantomEntity
    {
        public MoverEntity(Entity Source, Transform Default)
        {
            this._Source = Source;
            this._Terminal = new InTerminal<Transform>();
            this._Default = Default;
        }

        /// <summary>
        /// Gets the entity that is moved. This should be a phantom entity.
        /// </summary>
        public Entity Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the default transform used if the input terminal is inactive.
        /// </summary>
        public Transform Default
        {
            get
            {
                return this._Default;
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
        private Transform _Default;
    }
}