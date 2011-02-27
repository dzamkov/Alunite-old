using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// Contains functions for finding the optimal sphere partioning of a set
    /// of leaf nodes where each node is given a bounding sphere. A node can contain
    /// the optimal sphere partioning of a set of leaves if the two leaves with the smallest
    /// common bounding sphere are grouped in a single node and all subnodes contain the
    /// optimal sphere partioning of their set of leaves.
    /// </summary>
    /// <remarks>Comparing nodes with optimal sphere partioning is a good way of finding patterns in a set of points.</remarks>
    public static class OSP
    {
        /// <summary>
        /// Input to an OSP finder.
        /// </summary>
        public interface IOSPInput<TNode, TScalar>
        {
            /// <summary>
            /// Gets the subnodes in a node, if the node is a compound node, as opposed to a leaf node.
            /// </summary>
            bool GetSubnodes(TNode Node, out TNode A, out TNode B);

            /// <summary>
            /// Gets if the scalar value A is larger than B.
            /// </summary>
            bool Greater(TScalar A, TScalar B);

            /// <summary>
            /// Gets the diameter of a node.
            /// </summary>
            TScalar GetDiameter(TNode Node);

            /// <summary>
            /// Gets the distance between two nodes at the closest points on their bounding spheres.
            /// </summary>
            TScalar GetShortDistance(TNode A, TNode B);

            /// <summary>
            /// Gets the distance between two nodes at the furthest points on their bounding spheres. This is equivalent to
            /// the diameter of the bounding sphere containing both nodes.
            /// </summary>
            TScalar GetLongDistance(TNode A, TNode B);

            /// <summary>
            /// Creates a compound node with the specified subnodes.
            /// </summary>
            TNode CreateCompound(TNode A, TNode B);
        }

        /// <summary>
        /// Creates an OSP node of the specified leaf nodes.
        /// </summary>
        public static TNode Create<TInput, TNode, TScalar>(TInput Input, IEnumerable<TNode> Nodes)
            where TInput : IOSPInput<TNode, TScalar>
        {
            // Insert leafs into OSP node one at a time.
            TNode cur = default(TNode);
            IEnumerator<TNode> en = Nodes.GetEnumerator();
            if (en.MoveNext())
            {
                cur = en.Current;
                while (en.MoveNext())
                {
                    cur = Combine<TInput, TNode, TScalar>(Input, cur, en.Current);
                }
            }
            return cur;
        }

        /// <summary>
        /// Combines two OSP nodes. This function will be faster if A is the more complex node.
        /// </summary>
        public static TNode Combine<TInput, TNode, TScalar>(TInput Input, TNode A, TNode B)
            where TInput : IOSPInput<TNode, TScalar>
        {
            // Get sub nodes
            TNode subc;
            TNode subd;
            if (!Input.GetSubnodes(A, out subc, out subd))
            {
                if (!Input.GetSubnodes(B, out subc, out subd))
                {
                    // Trivial case, both nodes are leaves
                    return Input.CreateCompound(A, B);
                }

                // Swap A and B so that subc and subd belong to A
                TNode temp = A;
                A = B;
                B = temp;
            }

            // If the distance between them is greater than either diameter, the node containing both must be OSP
            TScalar dis = Input.GetShortDistance(A, B);
            if (Input.Greater(dis, Input.GetDiameter(A)) && Input.Greater(dis, Input.GetDiameter(B)))
            {
                return Input.CreateCompound(A, B);
            }

            // Create a node between C, D and B with the smallest possible diameter
            TScalar cddis = Input.GetLongDistance(subc, subd);
            TScalar cbdis = Input.GetLongDistance(subc, B);
            TScalar dbdis = Input.GetLongDistance(subd, B);
            if (Input.Greater(cbdis, cddis))
            {
                if (Input.Greater(dbdis, cddis))
                {
                    return Input.CreateCompound(A, B);
                }
                else
                {
                    return Input.CreateCompound(Combine<TInput, TNode, TScalar>(Input, subd, B), subc);
                }
            }
            else
            {
                if (Input.Greater(cbdis, dbdis))
                {
                    return Input.CreateCompound(Combine<TInput, TNode, TScalar>(Input, subd, B), subc);
                }
                else
                {
                    return Input.CreateCompound(Combine<TInput, TNode, TScalar>(Input, subc, B), subd);
                }
            }
        }
    }
}