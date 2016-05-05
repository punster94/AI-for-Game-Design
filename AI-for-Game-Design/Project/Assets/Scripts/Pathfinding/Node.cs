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

        // Allows storing a pointer to another node.
        public class NodePointer
        {
            Node start, finish;
            int distance;
            public NodePointer(Node from, Node target, int dist)
            {
                start = from;
                finish = target;
                distance = dist;
            }

            public Node getTarget()
            {
                return finish;
            }

            public Node getStart()
            {
                return start;
            }

            public int getDist()
            {
                return distance;
            }

            public override bool Equals(object obj)
            {
                Node.NodePointer n = obj as NodePointer;
                if (n == null)
                    return false;
                return n.start == start && n.finish == finish;
            }

            public override int GetHashCode()
            {
                return 57 + start.GetHashCode() * 43 + finish.GetHashCode();
            }
        }


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
        private SquareType terrainType;
        public SquareType TerrainType { get { return terrainType; } }
        private float edgePenalty;

        /// <summary>
        /// Initializes the node for pathfinding.
        /// </summary>
        public void initPathfinding(bool isPathfinding)
        {
            if (isPathfinding)
                Visited = Occupied;
            else
                Visited = false;

            CameFrom = null;
            realCost = double.PositiveInfinity;
            heuristicCost = double.PositiveInfinity;
        }

        // Sadly, we need to be able to reserve nodes, so we need a setter.
        private bool occupied = false;
        public bool Occupied
        {
            get
            {
                return occupied;
            }
            set
            {
                occupied = value;
            }
        }

        private Unit occupier;
        public Unit Occupier
        {
            get
            {
                return occupier;
            }

            set
            {
                occupier = value;
                Occupied = value != null;
            }
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


        public enum SquareType
        {
            Slippery, TableDef, Sandpaper, Unwalkable
        }

        public static SquareType randWalkState()
        {
            float randVal = Random.value;
            if (randVal < 0.10)
                return SquareType.Slippery;
            if (randVal < 0.15)
                return SquareType.Sandpaper;
            if (randVal < 0.85)
                return SquareType.TableDef;

            return SquareType.Unwalkable;
        }

        public static SquareType randWalkable()
        {
            float randVal = Random.value;
            if (randVal < 0.13)
                return SquareType.Slippery;
            if (randVal < 0.19)
                return SquareType.Sandpaper;
            return SquareType.TableDef;
        }

        public bool isWalkable()
        {
            return terrainType != SquareType.Unwalkable;
        }

        /// <summary>
        /// Returns the cost of a given WalkState, where Unwalkable = +inf.
        /// Must be >= 1, to preserve Manhatten Distance Heuristic
        /// </summary>
        /// <param name="w">The walkstate to measure the cost of.</param>
        /// <returns>Slippery = 1.0, TableDef = 2.0, Sandpaper = 3.0, HotSquare = 5.0, Unwalkable = pos inf</returns>
        public float costOfWalkState(SquareType w)
        {
            switch (w)
            {
                case SquareType.Slippery:
                    return 1.0f;
                case SquareType.TableDef:
                    return 2.0f;
                case SquareType.Sandpaper:
                    return 5.0f; 
                case SquareType.Unwalkable:
                    return float.PositiveInfinity;
            }
            return float.PositiveInfinity;
        }

        /// <summary>
        /// Constructs a Node.
        /// </summary>
        /// <param name="floorImg">The image which the node will use to display itself if set to do so.</param>
        /// <param name="position">Where the node is positioned in world space.</param>
        /// <param name="gridPos">The node position in the graph space.</param>
        /// <param name="scale">The node's visible scale.</param>
        /// <param name="num">The node's internal number.</param>
        /// <param name="vis">Whether the node is visible or not. Default is false.</param>
        public Node(GameObject parent, Sprite floorImg, Vector2 position, IntVec2 gridPos, SquareType typeOfTerrain, float scale = 0.75f, int num = 0, bool vis = true)
        {
            pos = position;
            this.gridPos = gridPos;
            GameObject drawer = new GameObject("Node " + pos);
            drawer.isStatic = true;
            drawer.transform.parent = parent.transform;

            //set sprite back in z, so it draws beneath everything.
            drawer.transform.position = new Vector3(position.x, position.y, 1);

            spriteDraw = drawer.AddComponent<SpriteRenderer>();
            spriteDraw.sprite = floorImg;
            drawer.tag = gameTag;
            // Draw a little bigger than scale.
            spriteDraw.transform.localScale = new Vector2(scale * 1.35f, scale * 1.35f);

            // Draw with 50% alpha-white.
            //spriteDraw.color = defColor;
            //terrainType = randWalkState();
            terrainType = typeOfTerrain;
            edgePenalty = costOfWalkState(terrainType);
            resetColor();
            //spriteDraw.color = Random.ColorHSV(0.2f, 1.0f, 0.1f, 0.7f, 0.2f, 1.0f);

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
        /// Color of the node. Debug mode.
        /// </summary>
        public Color getColor()
        {
            return spriteDraw.color;
        }


        /// <summary>
        /// Resets color of the node to real color. Debug mode.
        /// </summary>
        public void resetColor()
        {
            switch (terrainType)
            {
                case SquareType.Slippery:
                    spriteDraw.color = Color.blue;
                    break;
                case SquareType.TableDef:
                    spriteDraw.color = Color.HSVToRGB(0.083333f, 1, 0.59f);
                    break;
                case SquareType.Sandpaper:
                    spriteDraw.color = Color.HSVToRGB(0.083333f, 1, 0.30f);
                    break;
                default:
                    spriteDraw.color = Color.black;
                    break;
            }
        }

        /// <summary>
        /// Adds an edge to this node.
        /// </summary>
        /// <param name="e">The edge to add.</param>
        private void addEdge(Edge e)
        {

            if (e.getNode().TerrainType == SquareType.Unwalkable ||
                            TerrainType == SquareType.Unwalkable)
                return;

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
            //helps w/diagonal movement.
            //float avgPen = (other.edgePenalty + this.edgePenalty) / 2.0f;
            //this.addEdge(new Edge(other, weight * avgPen));
            //other.addEdge(new Edge(this, weight * avgPen));

            this.addEdge(new Edge(other, weight * other.edgePenalty));
            other.addEdge(new Edge(this, weight * this.edgePenalty));
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
            //Just in case it isn't destroyed right now, make it invisible.
            Visible = false;
            Object.Destroy(spriteDraw.gameObject);

#if DEBUG_NODE_TEXT
            Object.Destroy(visNodeNum.gameObject);
#endif
        }
        
        public static int range(Node a, Node b)
        {
            int rX = a.getGridPos().x - b.getGridPos().x;
            int rY = a.getGridPos().y - b.getGridPos().y;

            if (rX < 0)
                rX = -rX;
            if (rY < 0)
                rY = -rY;

            return rX + rY;
        }
    }
}
