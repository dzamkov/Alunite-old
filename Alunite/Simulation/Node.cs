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

    /// <summary>
    /// A mapping of nodes in one entity to those in another, usually larger and more complex entity.
    /// </summary>
    public abstract class NodeMap
    {
        /// <summary>
        /// Gets the identity node map.
        /// </summary>
        public static IdentityNodeMap Identity
        {
            get
            {
                return IdentityNodeMap.Singleton;
            }
        }

        /// <summary>
        /// Gets a new lazy node map.
        /// </summary>
        public static LazyNodeMap Lazy
        {
            get
            {
                return new LazyNodeMap();
            }
        }

        /// <summary>
        /// Finds the corresponding node for the given local node.
        /// </summary>
        public abstract T Lookup<T>(T Node)
            where T : Node, new();
    }

    /// <summary>
    /// A node map where lookup operations simply return the same node that is given.
    /// </summary>
    public class IdentityNodeMap : NodeMap
    {
        internal IdentityNodeMap()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly IdentityNodeMap Singleton = new IdentityNodeMap();

        public override T Lookup<T>(T Node)
        {
            return Node;
        }
    }

    /// <summary>
    /// A node map that only produces new nodes as they are looked up.
    /// </summary>
    public class LazyNodeMap : NodeMap
    {
        public LazyNodeMap()
        {
            this._Nodes = new Dictionary<Node, Node>();
        }

        public override T Lookup<T>(T Node)
        {
            Node res;
            if (this._Nodes.TryGetValue(Node, out res))
            {
                return res as T;
            }
            else
            {
                T tres = new T();
                this._Nodes[Node] = tres;
                return tres;
            }
        }

        private Dictionary<Node, Node> _Nodes;
    }
}