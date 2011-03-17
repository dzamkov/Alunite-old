using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An entity made from the combination of other entities. Note that each entity has its nodes remapped to avoid
    /// conflicts when reusing entities. If multiple physical entities take up the same space, they will be superimposed (which is usually
    /// not the desired effect, but it is the only logical one).
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
            /// A mapping of nodes from internally in the entity to externally in the compound entity.
            /// </summary>
            public NodeMap NodeMap;

            /// <summary>
            /// The entity for this element.
            /// </summary>
            public Entity Entity;
        }

        /// <summary>
        /// Adds an entity to the compound and returns a terminal map that can be used to get global nodes for each
        /// entity's local node.
        /// </summary>
        public NodeMap Add(Entity Entity)
        {
            if (Entity != Entity.Null)
            {
                NodeMap tm = this._Elements.Count == 0 ? (NodeMap)NodeMap.Identity : NodeMap.Lazy;
                this._Elements.Add(new Element()
                {
                    NodeMap = tm,
                    Entity = Entity
                });
                return tm;
            }
            else
            {
                return NodeMap.Identity;
            }
        }

        public override bool Phantom
        {
            get
            {
                foreach (Element e in this._Elements)
                {
                    if (!e.Entity.Phantom)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override MassAggregate Aggregate
        {
            get
            {
                MassAggregate n = MassAggregate.Null;
                foreach (Element e in this._Elements)
                {
                    n = n + e.Entity.Aggregate;
                }
                return n;
            }
        }

        private List<Element> _Elements;
    }
}