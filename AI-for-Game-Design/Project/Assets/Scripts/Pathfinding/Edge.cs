using UnityEngine;
using System.Collections;

namespace Graph
{
    /// <summary>
    /// Directed edge class.
    /// </summary>
    public class Edge
    {
        float weight;
        Node to;
        
        /// <param name="pointTo">Edge to point to.</param>
        /// <param name="weight">Weight of edge, def = 1.</param>
        public Edge(Node pointTo, float weight = 1)
        {
            this.weight = weight;
            to = pointTo;
        }

        public float getWeight() { return weight; }
        public Node getNode() { return to; }
        public Vector2 getPos() { return to.getPos(); }

        public override bool Equals(object obj)
        {
            Edge e = obj as Edge;
            if (e == null)
                return false;

            return weight == e.weight && to == e.to;
        }

        public override int GetHashCode()
        {
            return weight.GetHashCode();
        }
    }
}
