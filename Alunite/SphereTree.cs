using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Partions and organizes spherical nodes containing data into a bounding volume hierarchy.
    /// </summary>
    public abstract class SphereTree<TNode>
    {
        /// <summary>
        /// Gets the position and radius of the specified node.
        /// </summary>
        public abstract void GetBound(TNode Node, out Vector Position, out double Radius);

        /// <summary>
        /// Gets the subnodes for a node, if the node is a compound node.
        /// </summary>
        public abstract bool GetSubnodes(TNode Node, ref TNode A, ref TNode B);

        /// <summary>
        /// Creates a compound node containing the two specified subnodes.
        /// </summary>
        public abstract TNode CreateCompound(TNode A, TNode B);
    }

    /// <summary>
    /// A sphere tree that automatically creates and maintains compound nodes which track position, radius and subnodes.
    /// </summary>
    public abstract class SimpleSphereTree<TLeaf> : SphereTree<SimpleSphereTreeNode<TLeaf>>
    {
        /// <summary>
        /// Gets the bounding sphere for a leaf.
        /// </summary>
        public abstract void GetBound(TLeaf Leaf, out Vector Position, out double Radius);

        /// <summary>
        /// Creates a leaf node for a leaf.
        /// </summary>
        public SimpleSphereTreeNode<TLeaf> Create(TLeaf Leaf)
        {
            return new SimpleSphereTreeNode<TLeaf>._Leaf()
            {
                Leaf = Leaf
            };
        }

        public sealed override void GetBound(SimpleSphereTreeNode<TLeaf> Node, out Vector Position, out double Radius)
        {
            Node._GetBound(this, out Position, out Radius);
        }

        public sealed override bool GetSubnodes(SimpleSphereTreeNode<TLeaf> Node, ref SimpleSphereTreeNode<TLeaf> A, ref SimpleSphereTreeNode<TLeaf> B)
        {
            var c = Node as SimpleSphereTreeNode<TLeaf>._Compound;
            if (c != null)
            {
                A = c.A;
                B = c.B;
                return true;
            }
            return false;
        }

        public sealed override SimpleSphereTreeNode<TLeaf> CreateCompound(SimpleSphereTreeNode<TLeaf> A, SimpleSphereTreeNode<TLeaf> B)
        {
            Vector posa; double rada; this.GetBound(A, out posa, out rada);
            Vector posb; double radb; this.GetBound(B, out posb, out radb);
            Vector dir = posb - posa;
            double dis = dir.Length;
            dir *= 1.0 / dis;

            return new SimpleSphereTreeNode<TLeaf>._Compound()
            {
                A = A,
                B = B,
                Radius = (dis + rada + radb) * 0.5,
                Position = posa + dir * (dis * 0.5 + rada)
            };
        }
    }

    /// <summary>
    /// A node for a SimpleSphereTree.
    /// </summary>
    public abstract class SimpleSphereTreeNode<TLeaf>
    {
        internal abstract void _GetBound(SimpleSphereTree<TLeaf> Tree, out Vector Position, out double Radius);

        internal class _Leaf : SimpleSphereTreeNode<TLeaf>
        {
            internal override void _GetBound(SimpleSphereTree<TLeaf> Tree, out Vector Position, out double Radius)
            {
                Tree.GetBound(this.Leaf, out Position, out Radius);
            }

            public TLeaf Leaf;
        }

        internal class _Compound : SimpleSphereTreeNode<TLeaf>
        {
            internal override void _GetBound(SimpleSphereTree<TLeaf> Tree, out Vector Position, out double Radius)
            {
                Position = this.Position;
                Radius = this.Radius;
            }

            public Vector Position;
            public double Radius;
            public SimpleSphereTreeNode<TLeaf> A;
            public SimpleSphereTreeNode<TLeaf> B;
        }
    }
}