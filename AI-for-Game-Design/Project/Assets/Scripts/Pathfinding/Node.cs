//#define DEBUG_NODE_TEXT   //Show node number
//#define DEBUG_NODE_EDGES  //Show edges between nodes
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Graph
{
    /// <summary>
    /// Traditional adjacency list representation of node.
    /// Nodes of equal position are assumed to be equal.
    /// </summary>
    public class Node
    {
        List<Edge> edgeList = new List<Edge>();
        Vector2 pos;
        IntVec2 gridPos;
        SpriteRenderer spriteDraw;

#if DEBUG_NODE_EDGES
        LineRenderer lineDraw;
#endif
#if DEBUG_NODE_TEXT
        GUIText visNodeNum;
#endif

        // public fields used for pathfinding only!
        public bool Visited { get; set; }
        public Node CameFrom { get; set; }
        public double realCost { get; set; }
        public double heuristicCost { get; set; }
        private int number;
        public int Number { get { return number; } }

        /// <summary>
        /// Initializes the node for pathfinding.
        /// </summary>
        public void initPathfinding()
        {
            Visited = false;
            CameFrom = null;
            realCost = double.PositiveInfinity;
            heuristicCost = double.PositiveInfinity;
        }

        static public readonly string gameTag = "NodeTag";
        static public readonly int maxEdges = 8;

        /// <summary>
        /// Default color, blu-ish.
        /// </summary>
        static public readonly Color defColor = new Color(0.5f, 0.2f, 1, 0.5f);
        /// <summary>
        /// Default start color, red-ish.
        /// </summary>
        static public readonly Color startColor = new Color(1, 0.2f, 0, 0.5f);
        /// <summary>
        /// Default end color, green-ish.
        /// </summary>
        static public readonly Color endColor = new Color(0, 1, 0, 0.5f);

        /// <summary>
        /// Constructs a Node.
        /// </summary>
        /// <param name="floorImg">The image which the node will use to display itself if set to do so.</param>
        /// <param name="position">Where the node is positioned in world space.</param>
        /// <param name="gridPos">The node position in the graph space.</param>
        /// <param name="scale">The node's visible scale.</param>
        /// <param name="num">The node's internal number.</param>
        /// <param name="vis">Whether the node is visible or not. Default is false.</param>
        public Node(Sprite floorImg, Vector2 position, IntVec2 gridPos, float scale = 0.75f, int num = 0, bool vis = true)
        {
            pos = position;
            this.gridPos = gridPos;
            GameObject drawer = new GameObject("Node " + pos);

            //set sprite back in z, so it draws beneath everything.
            drawer.transform.position = new Vector3(position.x, position.y, 1);

            spriteDraw = drawer.AddComponent<SpriteRenderer>();
            spriteDraw.sprite = floorImg;
            drawer.tag = gameTag;
            // Draw a little bigger than scale.
            spriteDraw.transform.localScale = new Vector2(scale * 1.35f, scale * 1.35f);

            // Draw with 50% alpha-white.
            spriteDraw.color = defColor;

            CameFrom = null;
            number = num;

#if DEBUG_NODE_EDGES
            lineDraw = drawer.AddComponent<LineRenderer>();
            lineDraw.SetWidth(0.1f, 0.1f);
            lineDraw.SetVertexCount(maxEdges * 2 + 1);
            for (int i = 0; i < 17; i++)
                lineDraw.SetPosition(i, pos);
#endif

#if DEBUG_NODE_TEXT
            GameObject guiText = new GameObject("Debug text on Node " + pos);
            Vector2 guiPos = new Vector2(Camera.main.WorldToScreenPoint(pos).x / Camera.main.pixelWidth,
                                         Camera.main.WorldToScreenPoint(pos).y / Camera.main.pixelHeight);
            guiText.transform.position = guiPos;
            visNodeNum = guiText.AddComponent<GUIText>();
            visNodeNum.text = num.ToString();
            visNodeNum.anchor = TextAnchor.MiddleCenter;
#endif
            // Do not move this, everything must be defined before calling Visible.
            Visible = vis;
        }

        /// <summary>
        /// Sets the node to visible or not.
        /// </summary>
        public bool Visible
        {
            get
            {
                return spriteDraw.enabled;
            }
            set
            {
                spriteDraw.enabled = value;

#if DEBUG_NODE_EDGES
                lineDraw.enabled = value;
#endif
#if DEBUG_NODE_TEXT
                visNodeNum.enabled = value;
#endif
            }
        }

        /// <summary>
        /// Color of the node. Debug mode.
        /// </summary>
        /// <param name="c">The color to display when debugging.</param>
        public void setColor(Color c)
        {
            spriteDraw.color = c;
        }

        /// <summary>
        /// Adds an edge to this node.
        /// </summary>
        /// <param name="e">The edge to add.</param>
        public void addEdge(Edge e)
        {
            if (edgeList.Count == maxEdges)
            {
                string errmsg = "tried to add more than " + maxEdges + " edges! " + ToString() + e.getNode().ToString() + "\n";
                foreach (Edge edge in edgeList)
                    errmsg += edge.getNode().ToString();
                throw new System.Exception(errmsg);
            }

#if DEBUG_NODE_EDGES
            lineDraw.SetPosition(edgeList.Count * 2 + 1, e.getPos());
            lineDraw.SetPosition(edgeList.Count * 2 + 2, pos);
#endif
            // Do not move this. lineDraw will out of bounds otherwise.
            edgeList.Add(e);
        }

        /// <summary>
        /// Adds an edge to this node.
        /// </summary>
        /// <param name="e">The edge to add.</param>
        public void addEdge(Node n, float weight = 1)
        {
            this.addEdge(new Edge(n, weight));
        }

        /// <summary>
        /// Returns all edges in the node.
        /// </summary>
        /// <returns>All edges in the node.</returns>
        public Edge[] getEdges()
        {
            return edgeList.ToArray();
        }

        /// <summary>
        /// Adds a bi-directional edge to this node, weighted evenly.
        /// </summary>
        /// <param name="other">The other node to share the edge with.</param>
        /// <param name="weight">The weight for the edge, default value is 1.</param>
        public void addBidirEdge(Node other, float weight = 1)
        {
            this.addEdge(new Edge(other, weight));
            other.addEdge(new Edge(this, weight));
        }

        /// <summary>
        /// Returns the position of the node in world space.
        /// </summary>
        /// <returns>The position of the node in world space.</returns>
        public Vector2 getPos() { return pos; }

        /// <summary>
        /// Returns the grid position of the node.
        /// </summary>
        /// <returns>The grid pos of the node in world space.</returns>
        public IntVec2 getGridPos() { return gridPos; }

        public override string ToString()
        {
            return (pos.ToString());
        }

        /// <summary>
        /// Equality is assumed when the other position = this position.
        /// </summary>
        public override bool Equals(object obj)
        {
            Node other = obj as Node;
            if (other == null)
                return false;
            return pos.Equals(other.pos);
        }

        /// <summary>
        /// Hash based on the current position.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 43;
                hash = hash * 29 + pos.GetHashCode();
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Destroys this node, leaving it in an unstable state.
        /// </summary>
        public void destroyThisNode()
        {
            Object.Destroy(spriteDraw.gameObject);

#if DEBUG_NODE_TEXT
            Object.Destroy(visNodeNum.gameObject);
#endif
        }
    }
}
