using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity made from the combination of other entities. Note that each entity has its terminals remapped to avoid
    /// conflicts when reusing entities.
    /// </summary>
    public class CompoundEntity : Entity
    {
        public CompoundEntity()
        {
            this._Elements = new List<Element>();
        }

        /// <summary>
        /// Gets the entities that make up this compound entity.
        /// </summary>
        public IEnumerable<Element> Elements
        {
            get
            {
                return this._Elements;
            }
        }

        /// <summary>
        /// An element within a compound entity.
        /// </summary>
        public struct Element
        {
            /// <summary>
            /// A mapping of terminals from internally in the entity to externally in the compound entity.
            /// </summary>
            public TerminalMap TerminalMap;

            /// <summary>
            /// The entity for this element.
            /// </summary>
            public Entity Entity;
        }

        /// <summary>
        /// Adds an entity to the compound and returns a terminal map that can be used to get global terminals for each
        /// entities local terminals.
        /// </summary>
        public TerminalMap Add(Entity Entity)
        {
            TerminalMap tm = this._Elements.Count == 0 ? (TerminalMap)TerminalMap.Identity : TerminalMap.Lazy;
            this._Elements.Add(new Element()
            {
                TerminalMap = tm,
                Entity = Entity
            });
            return tm;
        }

        private List<Element> _Elements;
    }
}