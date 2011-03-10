using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity made from the combination of other entities. Note that each entity has its terminals remapped to avoid
    /// conflicts when reusing entities.
    /// </summary>
    public class CompoundEntity
    {
        public CompoundEntity()
        {
            this._Elements = new List<_Element>();
        }

        /// <summary>
        /// Gets the entities that make up this compound entity.
        /// </summary>
        public IEnumerable<Entity> Elements
        {
            get
            {
                foreach (_Element e in this._Elements)
                {
                    yield return e.Entity;
                }
            }
        }

        /// <summary>
        /// Adds an entity to the compound and returns a terminal map that can be used to get global terminals for each
        /// entities local terminals.
        /// </summary>
        public TerminalMap Add(Entity Entity)
        {
            TerminalMap tm = this._Elements.Count == 0 ? (TerminalMap)TerminalMap.Identity : TerminalMap.Lazy;
            this._Elements.Add(new _Element()
            {
                TerminalMap = tm,
                Entity = Entity
            });
            return tm;
        }

        private List<_Element> _Elements;
    }

    /// <summary>
    /// An entity used within a compound entity.
    /// </summary>
    internal struct _Element
    {
        /// <summary>
        /// A mapping of terminals from locally to globally within the compound entity.
        /// </summary>
        public TerminalMap TerminalMap;

        /// <summary>
        /// The entity for this element.
        /// </summary>
        public Entity Entity;
    }
}