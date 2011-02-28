using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Partions and organizes spherical nodes containing data into a bounding volume hierarchy. Note that this object does not contain
    /// an actual tree, just the properties of the tree's its nodes make.
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

        /// <summary>
        /// Creates a well-balanced tree containing the given nodes.
        /// </summary>
        public TNode Create(IEnumerable<TNode> Nodes)
        {
            TNode cur = default(TNode);
            IEnumerator<TNode> e = Nodes.GetEnumerator();
            if (e.MoveNext())
            {
                cur = e.Current;
                while (e.MoveNext())
                {
                    cur = this.Insert(cur, e.Current);
                }
            }
            return cur;
        }

        /// <summary>
        /// Inserts node B into tree A leaving them well-balanced.
        /// </summary>
        public TNode Insert(TNode A, TNode B)
        {
            TNode suba = default(TNode);
            TNode subb = default(TNode);
            if (!this.GetSubnodes(A, ref suba, ref subb))
            {
                return this.CreateCompound(A, B);
            }

            Vector possuba; double radsuba;
            Vector possubb; double radsubb;
            Vector posb; double radb;
            Vector posa; double rada;
            this.GetBound(suba, out possuba, out radsuba);
            this.GetBound(subb, out possubb, out radsubb);
            this.GetBound(A, out posa, out rada);
            this.GetBound(B, out posb, out radb);

            double subdis = rada * 2.0;
            double subadis = (possuba - posb).Length + radsuba + radb;
            double subbdis = (possubb - posb).Length + radsubb + radb;
            if (subdis < subadis)
            {
                if (subdis < subbdis)
                {
                    return this.CreateCompound(A, B);
                }
                else
                {
                    return this.CreateCompound(this.Insert(subb, B), suba);
                }
            }
            else
            {
                if (subadis < subbdis)
                {
                    return this.CreateCompound(this.Insert(suba, B), subb);
                }
                else
                {
                    return this.CreateCompound(this.Insert(subb, B), suba);
                }
            }
        }
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

            double rad = (dis + rada + radb) * 0.5;
            Vector pos = posa + dir * (rad - rada);

            return new SimpleSphereTreeNode<TLeaf>._Compound()
            {
                A = A,
                B = B,
                Radius = rad,
                Position = pos
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