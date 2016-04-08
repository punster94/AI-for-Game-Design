//#define DEBUG_PATHFINDER_UPDATELOOP // allows graph to auto-update w/physics, display manual paths.
//#define DEBUG_PATHFINDER_DRAWDEBUG  // draws debug paths and shows start/end nodes.
using UnityEngine;
using System.Collections.Generic;

namespace Graph
{
    /// <summary>
    /// Initializes a graph, and pathfinds it.
    /// </summary>
    public class PathFinder : MonoBehaviour
    {
        private bool isInitialized = false;
        private Vector2 lowerLeftBound;
        private Vector2 upperRightBound;
        private int numXNodes;
        private int numYNodes;
        private int numValidNodes;
        private float nodeDensity;

        public enum Paths { quadDir, octDir }
        public static Paths allowedPaths = Paths.quadDir;

        /// TODO: Implement this in PCG
        /// <summary>
        /// Generates a pair of list of spawn points. Assumes graph already generated.
        /// Picks either top right, bottom left, or top left, bottom right pairs.
        /// Either key/value can be enemy/friend spawn points.
        /// </summary>
        /// <param name="spawnPointsPerPlayer">Number of spawn points to provide each player</param>
        /// <returns>A pair of spawn points (lists of valid node points), empty if failed.</returns>
        public KeyValuePair<List<Unit>, List<Unit>> getSpawnPoints(int spawnPointsPerPlayer = 5)
        {
            List<Unit> enemy = new List<Unit>(), friend = new List<Unit>();

            Vector2 locusEnemy, locusFriend;

            // pick top right, bottom left pair
            if (Random.value < 0.5)
            {
                locusEnemy = upperRightBound;
                locusFriend = lowerLeftBound;
            }
            // pick bottom right, top left pair
            else
            {
                locusEnemy = new Vector2(upperRightBound.x, lowerLeftBound.y);
                locusFriend = new Vector2(lowerLeftBound.x, upperRightBound.y);
            }
            


            return new KeyValuePair<List<Unit>, List<Unit>>(enemy, friend);
        }

        // Node array is stored as Node[inverse-y][x].
        private Node[][] nodeArr;
        private float radii;
        private Sprite nodeImg;

        public void Start()
        {
            nodeImg = Resources.Load<Sprite>("PathLoc");
        }

        private static int LAYER_FILTER_MASK = LayerMask.GetMask("Walls");

        /// <summary>
        /// Initializes the graph.
        /// Important: Since this uses physics, we cannot use a constructor.
        /// Instead, we have to call this on the already constructed object.
        /// </summary>
        /// <param name="lowLeftBound">The lower left bound of the map in world space.</param>
        /// <param name="upRightBound">The upper right bound of the map in world space.</param>
        /// <param name="pathWidth">How wide to make the path. Smaller values mean we can place nodes in tighter spaces.</param>
        /// <param name="nodeDensity">How dense the nodes should be. Smaller values mean less nodes are placed.</param>
        /// <param name="startPos">The starting seed position of the graph.</param>
        public void initializeGraph(Vector2 lowLeftBound, Vector2 upRightBound, float pathWidth, float nodeDensity = 1.0f, Vector2 startPos = new Vector2())
        {
            lowerLeftBound = lowLeftBound;
            upperRightBound = upRightBound;

            this.nodeDensity = nodeDensity;

            // Determine proper numbers of nodes in the x direction.
            float xDist = lowerLeftBound.x - upperRightBound.x;
            int xDensity = Mathf.CeilToInt(Mathf.Abs(xDist * nodeDensity));

            // Same, but for y direction.
            float yDist = upperRightBound.y - lowerLeftBound.y;
            int yDensity = Mathf.CeilToInt(Mathf.Abs(yDist * nodeDensity));

            numXNodes = xDensity;
            numYNodes = yDensity;

            // Initialize the node array. Note the inverted y, normal x setup.
            nodeArr = new Node[yDensity][];
            for (int i = 0; i < yDensity; i++)
                nodeArr[i] = new Node[xDensity];

            radii = pathWidth / 2.0f;

            //floodFill(startPos);
            fillAll();

            numValidNodes = 0;

            isInitialized = true;
        }

        /// <summary>
        /// Returns a sorted list of nodes within a given endurance value.
        /// Performs a Dijkstra-like algorithm.
        /// </summary>
        /// <param name="endurance">The maximum endurance to follow out.</param>
        /// <returns>A sorted list of nodes within a given endurance value.</returns>
        public List<Node> nodesWithinEnduranceValue(Node startNode, float endurance = 8.0f)
        {
            List<Node> foundNodes = new List<Node>();
            MinPriorityQueue<Node> nodeList = new MinPriorityQueue<Node>();

            //Initializes nodes to ifinity realcost, heuristic, null camefrom, and not visited.
            foreach (Node[] arr in nodeArr)
                foreach (Node p in arr)
                    if (p != null)
                    {
                        p.initPathfinding();
                    }

            startNode.realCost = 0;
            nodeList.Enqueue(startNode, startNode.realCost);


            string encountered = "";
            string nodes = "";
            nodes += "Start node " + startNode.Number + "\n";
            encountered += "Start node " + startNode.Number + "\n";
            encountered += "endurance = " + endurance + "\n";

            while (nodeList.Count > 0)
            {
                //Pick the best looking node, by f-value.
                Node best = nodeList.Dequeue();
                double bestDist = best.realCost;

                encountered += "Node " + best.Number + " " + best.ToString() + "\n";
                nodes += "Node " + best.Number + "\n";
                
                best.Visited = true;
                foundNodes.Add(best);

                //string updateString = "updating: ";
                foreach (Edge e in best.getEdges())
                {
                    Node other = e.getNode();

                    //We already visited this node, move along,
                    if (other.Visited)
                        continue;

                    //Tentative distance.
                    double testDist = e.getWeight() + bestDist;

                    if (testDist > endurance)
                        continue;

                    //If the other node isn't in the priority queue, add it.
                    if (!nodeList.Contains(other))
                    {
                        other.CameFrom = best;
                        other.realCost = testDist;
                        nodeList.Enqueue(other, other.realCost);
                        encountered += "   added " + other.Number
                            + ", total estimated cost "
                            + (other.realCost) + "\n";
                        continue;
                    }
                    //If the other node was a bad path, and this one's better, replace it.
                    else if (other.realCost > testDist)
                    {
                        other.CameFrom = best;
                        other.realCost = testDist;
                        nodeList.Update(other, other.realCost);
                        encountered += "   updated " + other.Number
                            + ", total new estimated cost "
                            + (other.realCost) + "\n";
                    }
                }
            }
            Debug.Log(encountered);
            Debug.Log(nodes);

            return foundNodes;
        }

        /// <summary>
        /// Destroys the current graph, creates a new one with the same area.
        /// Graph must already be created to use this.
        /// Note that any node reference may become invalid when this occurs.
        /// </summary>
        public void onlineRecreateGraph()
        {
            if (!isInitialized)
                throw new UnassignedReferenceException("Pathfinder not initialized yet!");

            Debug.Log("Recreated Graph.");

            bool graphWasDisplayed = graphIsDisplayed();

            for (int y = 0; y < numYNodes; y++)
            {
                for (int x = 0; x < numXNodes; x++)
                    if (nodeArr[y][x] != null)
                    {
                        nodeArr[y][x].destroyThisNode();
                        nodeArr[y][x] = null;
                    }
            }

#if DEBUG_PATHFINDER_DRAWDEBUG
            //Invalidate this object's references to the old nodes.
            if (pathDrawer != null)
                UnityEngine.Object.Destroy(pathDrawer);
            manualEndNode = manualStartNode = null;
#endif

            numValidNodes = 0;
            fillAll();
            if (graphWasDisplayed)
                graphDisplay(true);
        }
        
        /// <summary>
        /// Finds the closest most-valid node to a given position.
        /// In worst case runs line tests on all created nodes in graph.
        /// </summary>
        /// <param name="loc">The location to start looking from.</param>
        /// <returns>The most valid node that's closest to the given location.</returns>
        private Node closestMostValidNode(Vector2 loc)
        {
            IntVec2 pair = WorldSpaceToArrPos(loc);

            // Trim the location down to the closest valid array index.
            int x, y;
            if (pair.x >= numXNodes)
                x = numXNodes - 1;
            else if (pair.x < 0)
                x = 0;
            else
                x = pair.x;

            if (pair.y >= numYNodes)
                y = numYNodes - 1;
            else if (pair.y < 0)
                y = 0;
            else
                y = pair.y;

            // Attempt to return if our selected location is unobstructed.
            // This will be the closest valid node.
            Node closest = unobstructed(x, y, loc);
            if (closest != null)
            {
                return closest;
            }
            // We've failed to find the closest node on the first try.
            // Try to avoid looking through graph by looking at the nearest four nodes.

            // up
            Node upNode = null;
            upNode = unobstructed(x, y - 1, loc);

            //down, left, right
            Node downNode = null;
            downNode = unobstructed(x, y + 1, loc);
            Node leftNode = null;
            leftNode = unobstructed(x - 1, y, loc);
            Node rightNode = null;
            rightNode = unobstructed(x + 1, y, loc);
            Node[] arr = { upNode, downNode, leftNode, rightNode };

            // Loop through the four nodes, return closest if possible.
            float minDist = float.PositiveInfinity;
            foreach (Node a in arr)
                if (a != null && Vector2.SqrMagnitude(loc - a.getPos()) < minDist)
                {
                    closest = a;
                    minDist = Vector2.SqrMagnitude(loc - a.getPos());
                }

            if (closest != null)
            {
                return closest;
            }

            // Do a check to make sure we aren't obstructed.
            // If we are, loop and pick the closest valid node.
            if (!Physics2D.OverlapPoint(loc, LAYER_FILTER_MASK))
            {
                // We've failed to find the closest unobstructed node.
                // loop through the graph and find the closest unobstructed object then.
                // This is a very expensive operation, but we only look at nodes that aren't
                // null and are closer than the previous distance.
                closest = null;
                minDist = float.PositiveInfinity;
                for (y = 0; y < numYNodes; y++)
                    for (x = 0; x < numXNodes; x++)
                    {
                        Node a = nodeArr[y][x];
                        if (a != null && Vector2.SqrMagnitude(loc - a.getPos()) < minDist
                            && !Physics2D.Linecast(a.getPos(), loc, LAYER_FILTER_MASK))
                        {
                            closest = a;
                            minDist = Vector2.SqrMagnitude(loc - a.getPos());
                        }
                    }

                if (closest != null)
                    return closest;
            }

            // Just return the closest invalid node.
            closest = null;
            minDist = float.PositiveInfinity;
            for (y = 0; y < numYNodes; y++)
                for (x = 0; x < numXNodes; x++)
                {
                    Node a = nodeArr[y][x];
                    if (a != null && Vector2.SqrMagnitude(loc - a.getPos()) < minDist)
                    {
                        closest = a;
                        minDist = Vector2.SqrMagnitude(loc - a.getPos());
                    }
                }

            return closest;
        }

        private Node unobstructed(int x, int y, Vector2 pos)
        {
            if (x < 0 || y < 0 || x >= numXNodes || y >= numYNodes || nodeArr[y][x] == null ||
               Physics2D.Linecast(nodeArr[y][x].getPos(), pos, LAYER_FILTER_MASK))
                return null;
            return nodeArr[y][x];
        }

        /// <summary>
        /// Performs AStar on the graph.
        /// </summary>
        /// <param name="pathStoreLoc">The path will be stored in this queue.</param>
        /// <param name="startPos">The starting position.</param>
        /// <param name="targetPos">The target position.</param>
        public void AStar(Queue<Node> pathStoreLoc, Vector2 startPos, Vector2 targetPos)
        {
            if (!isInitialized)
                throw new UnassignedReferenceException("Pathfinder not initialized yet!");
            Node start = closestMostValidNode(startPos);
            Node end = closestMostValidNode(targetPos);

            AStar(pathStoreLoc, start, end);
        }

        /// <summary>
        /// Performs AStar on the graph.
        /// </summary>
        /// <param name="pathStoreLoc">The path will be stored in this queue.</param>
        /// <param name="startNode">The starting node.</param>
        /// <param name="targetPos">The target position.</param>
        public void AStar(Queue<Node> pathStoreLoc, Node startNode, Vector2 targetPos)
        {
            if (!isInitialized)
                throw new UnassignedReferenceException("Pathfinder not initialized yet!");
            Node end = closestMostValidNode(targetPos);

            AStar(pathStoreLoc, startNode, end);
        }

        /// <summary>
        /// Note: This calculates the diagonal distance heuristic.
        /// Taken from http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html.
        /// </summary>
        /// <param name="startNode">Start node</param>
        /// <param name="endNode">End node</param>
        /// <returns></returns>
        public float DiagonalHeuristic(Node startNode, Node endNode)
        {
            float distX = Mathf.Abs(startNode.getGridPos().x - endNode.getGridPos().x);
            float distY = Mathf.Abs(startNode.getGridPos().y - endNode.getGridPos().y);

            return distX + distY - (2 - sqrt2) * Mathf.Min(distX, distY);
        }
        
        /// <summary>
        /// Note: This calculates the manhatten distance heuristic.
        /// </summary>
        /// <param name="startNode">Start node</param>
        /// <param name="endNode">End node</param>
        /// <returns></returns>
        public float ManhattenHeuristic(Node startNode, Node endNode)
        {
            float distX = Mathf.Abs(startNode.getGridPos().x - endNode.getGridPos().x);
            float distY = Mathf.Abs(startNode.getGridPos().y - endNode.getGridPos().y);

            return distX + distY;
        }

        /// <summary>
        /// A* on the graph.
        /// </summary>
        /// <param name="pathStoreLoc">The Queue to store the path in.</param>
        /// <param name="start">The starting node.</param>
        /// <param name="end">The ending node.</param>
        public void AStar(Queue<Node> pathStoreLoc, Node start, Node end)
        {
            MinPriorityQueue<Node> nodeList = new MinPriorityQueue<Node>();

            //Initializes nodes to ifinity realcost, heuristic, null camefrom, and not visited.
            foreach (Node[] arr in nodeArr)
                foreach (Node p in arr)
                    if (p != null)
                    {
                        p.initPathfinding();
                    }
            System.Func<Node, Node, float> Heuristic;
            if (allowedPaths == Paths.quadDir)
                Heuristic = ManhattenHeuristic;
            else if (allowedPaths == Paths.octDir)
                Heuristic = DiagonalHeuristic;
            else
                Heuristic = DiagonalHeuristic;

            start.CameFrom = null;
            start.heuristicCost = Heuristic(start, end);
            start.realCost = 0;
            nodeList.Enqueue(start, start.heuristicCost);

            string encountered = "";
            string nodes = "";
            nodes += "Start node " + start.Number + "\n";
            encountered += "Start node " + start.Number + "\n";
            nodes += "End node " + end.Number + "\n";
            encountered += "End node " + end.Number + "\n";

            while (nodeList.Count > 0)
            {
                //Pick the best looking node, by f-value.
                Node best = nodeList.Dequeue();
                double bestDist = best.realCost;

                encountered += "Node " + best.Number + " " + best.ToString() + "\n";
                nodes += "Node " + best.Number + "\n";

                //If this is the end, stop, show the path, and return it.
                if (best.Equals(end))
                {
                    ReconstructPath(pathStoreLoc, end);
                    ShowPath(pathStoreLoc);
                    encountered += "Finished!\n\nFinal dist: " 
                                + best.realCost + "\n";
                    Debug.Log(encountered);
                    Debug.Log(nodes);
                    return;
                }
                best.Visited = true;

                //string updateString = "updating: ";
                foreach (Edge e in best.getEdges())
                {
                    Node other = e.getNode();

                    //We already visited this node, move along,
                    if (other.Visited)
                        continue;

                    //Tentative distance.
                    double testDist = e.getWeight() + bestDist;

                    //If the other node isn't in the priority queue, add it.
                    if (!nodeList.Contains(other))
                    {
                        other.CameFrom = best;
                        other.realCost = testDist;
                        other.heuristicCost = Heuristic(other, end);
                        nodeList.Enqueue(other, other.realCost + other.heuristicCost);
                        encountered += "   added " + other.Number
                            + ", total estimated cost " 
                            + (other.realCost + other.heuristicCost) + "\n";
                        continue;
                    }
                    //If the other node was a bad path, and this one's better, replace it.
                    else if (other.realCost > testDist)
                    {
                        other.CameFrom = best;
                        other.realCost = testDist;
                        nodeList.Update(other, other.realCost + other.heuristicCost);
                        encountered += "   updated " + other.Number
                            + ", total new estimated cost "
                            + (other.realCost + other.heuristicCost) + "\n";
                    }
                }
            }
        }

        /// <summary>
        /// Reconstructs a path from the end to the start.
        /// </summary>
        /// <param name="path">Queue to store path in.</param>
        /// <param name="end">Ending node in graph</param>
        private void ReconstructPath(Queue<Node> path, Node end)
        {
            path.Clear();
            if (end == null)
                return;

            Stack<Node> reversePath = new Stack<Node>();
            Node cur = end;

            while (cur != null)
            {
                reversePath.Push(cur);
                cur = cur.CameFrom;
            }

            while (reversePath.Count > 0)
                path.Enqueue(reversePath.Pop());
        }

        /// <summary>
        /// Sets the entire graph to either display or not.
        /// </summary>
        /// <param name="visVal">If true, display graph, otherwise, keep graph hidden.</param>
        public void graphDisplay(bool visVal)
        {
            if (visVal == graphDisplayed)
                return;

            foreach (Node[] outList in nodeArr)
                foreach (Node p in outList)
                    if (p != null) p.Visible = visVal;

            graphDisplayed = visVal;
        }

        bool graphDisplayed = false;

        public bool graphIsDisplayed()
        {
            return graphDisplayed;
        }

        private Node manualStartNode = null, manualEndNode = null;
        private List<Node> overlayNodes;

        GameObject pathDrawer = null;

        /// <summary>
        /// The graph has a update function, for showing off pathfinding
        /// without using the subject as a starting node.
        /// Can also automatically update the graph to changes if enabled.
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown("y"))
            {
                if (overlayNodes == null)
                    overlayNodes = new List<Node>();

                foreach (Node n in overlayNodes)
                    if (n != null)
                        n.destroyThisNode();

                overlayNodes.Clear();

                foreach (Node n in nodesWithinEnduranceValue(closestMostValidNode(getMousePos()), 8))
                {
                    //make slightly smaller to show square off
                    Node q = new Node(nodeImg, n.getPos(), n.getGridPos(), radii * 1.75f);
                    q.setColor(new Color(0, 0.5f, 0, 0.75f));

                    overlayNodes.Add(q);
                }
            }
#if DEBUG_PATHFINDER_UPDATELOOP
            if (Input.GetKeyDown("y"))
            {
                //Build start node.
                if (manualStartNode == null)
                {
                    manualStartNode = closestMostValidNode(getMousePos());
                    manualStartNode.setColor(Node.startColor);
                }
                //Build end node, show path.
                else if (manualEndNode == null)
                {
                    manualEndNode = closestMostValidNode(getMousePos());
                    Queue<Node> Path = new Queue<Node>();
                    AStar(Path, manualStartNode, manualEndNode);
                    ShowPath(Path);
                }
                //Path showing, stop showing it.
                else
                {
                    manualStartNode.setColor(Node.defColor);
                    manualEndNode.setColor(Node.defColor);
                    manualEndNode = manualStartNode = null;
                    UnityEngine.Object.Destroy(pathDrawer);
                    pathDrawer = null;
                }
            }

            if (isAutoRepairing && !graphCheckingRunning)
                StartCoroutine(CheckGraphRow());
#endif
            }


#if DEBUG_PATHFINDER_UPDATELOOP
        private readonly bool isAutoRepairing = true;
        private bool graphCheckingRunning = false;

        System.Collections.IEnumerator CheckGraphRow()
        {
            graphCheckingRunning = true;
            for (int y = 0; y < numYNodes; y++)
            {
                for (int x = 0; x < numXNodes; x++)
                {
                    if (nodeArr[y][x] != null
                        && Physics2D.OverlapCircle(nodeArr[y][x].getPos(), radii, LAYER_FILTER_MASK))
                        onlineRecreateGraph();
                    else if (nodeArr[y][x] == null
                        && !Physics2D.OverlapCircle(ArrPosToWorldSpace(new IntVec2(x, y)), radii, LAYER_FILTER_MASK))
                        onlineRecreateGraph();
                }
                yield return null;
            }
            graphCheckingRunning = false;
        }
#endif

        private Vector2 getMousePos()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        /// <summary>
        /// Displays a path of nodes.
        /// </summary>
        /// <param name="Path">The path to display.</param>
        [System.Diagnostics.Conditional ("DEBUG_PATHFINDER_DRAWDEBUG")]
        public void ShowPath(Queue<Node> Path)
        {
            Node[] PathCopy = Path.ToArray();
            if (PathCopy.Length == 0)
                return;

            //If we were displaying a path, reset the colors.
            if (manualStartNode != null)
                manualStartNode.setColor(Node.defColor);
            if (manualEndNode != null)
                manualEndNode.setColor(Node.defColor);

            //Set the colors for the start and end nodes.
            manualStartNode = PathCopy[0];
            manualEndNode = PathCopy[PathCopy.Length - 1];
            manualEndNode.setColor(Node.endColor);
            manualStartNode.setColor(Node.startColor);

            //Recycle garbage path renderers if necessary.
            if (pathDrawer != null)
            {
                UnityEngine.Object.Destroy(pathDrawer);
            }

            pathDrawer = new GameObject("Path");

            //Set up the line renderer for the path.
            // We loop back once, so have double nodes + 1.
            LineRenderer drawer = pathDrawer.AddComponent<LineRenderer>();
            drawer.SetWidth(0.3f, 0.3f);
            drawer.SetColors(Color.green, Color.green);
            drawer.SetVertexCount(PathCopy.Length * 2 + 1);
            // Need this otherwise the width is wrong on first edge.
            drawer.SetPosition(0, PathCopy[0].getPos());

            for (int i = 0; i < PathCopy.Length; i++)
                drawer.SetPosition(i + 1, PathCopy[i].getPos());
            for (int i = PathCopy.Length - 1, j = PathCopy.Length + 1; i >= 0; i--, j++)
                drawer.SetPosition(j, PathCopy[i].getPos());
        }

        /// <summary>
        /// Fills all nodes in area and connects edges as necessesary.
        /// Does not have stack overflow problems, but may create a graph
        /// that isn't connected all the way through.
        /// </summary>
        private void fillAll()
        {
            for (int y = 0; y < numYNodes; y++)
            {
                for (int x = 0; x < numXNodes; x++)
                {
                    IntVec2 arrPos = new IntVec2(x, y);
                    Vector2 newPosV2 = ArrPosToWorldSpace(arrPos);
                    if (!Physics2D.OverlapCircle(newPosV2, radii, LAYER_FILTER_MASK))
                    {
                        nodeArr[y][x] = new Node(nodeImg, newPosV2, arrPos, radii * 2, numValidNodes++);

                        //add up-left edge (inverse y)
                        if (allowedPaths > Paths.quadDir)
                        {
                            if (x - 1 >= 0 && y - 1 >= 0
                                && nodeArr[y - 1][x - 1] != null
                                && isOkayToFloodDiag(arrPos, IntVec2.down, IntVec2.left))
                                nodeArr[y][x].addBidirEdge(nodeArr[y - 1][x - 1], sqrt2);
                            //add up-right edge (inverse y)
                            if (x + 1 < numXNodes && y - 1 >= 0
                                && nodeArr[y - 1][x + 1] != null
                                && isOkayToFloodDiag(arrPos, IntVec2.down, IntVec2.right))
                                nodeArr[y][x].addBidirEdge(nodeArr[y - 1][x + 1], sqrt2);
                        }
                        //add up edge
                        if (y - 1 >= 0
                            && nodeArr[y - 1][x] != null
                            && isOkayToFloodUDLR(arrPos, IntVec2.down, (Vector2)IntVec2.left))
                            nodeArr[y][x].addBidirEdge(nodeArr[y - 1][x], 1);
                        //add left edge
                        if (x - 1 >= 0
                            && nodeArr[y][x - 1] != null
                            && isOkayToFloodUDLR(arrPos, IntVec2.left, (Vector2)IntVec2.up))
                            nodeArr[y][x].addBidirEdge(nodeArr[y][x - 1], 1);
                    }
                }
            }
        }
        
        /// <summary>
        /// Helper for floodFill.
        /// </summary>
        /// <param name="seedPos">Where to start seeding from.</param>
        private void floodFill(Vector2 seedPos)
        {
            IntVec2 startPos = WorldSpaceToArrPos(seedPos);
            floodFill(startPos);
        }

        private const float sqrt2 = 1.41421356237f;

        /// <summary>
        /// Classic floodfill algorithm.
        /// Has problems with stack overflows.
        /// Checks diagnonals, too.
        /// </summary>
        /// <param name="startPos">The current seed.</param>
        private void floodFill(IntVec2 startPos)
        {
            Node thisNode = new Node(nodeImg, ArrPosToWorldSpace(startPos), startPos, radii * 2, numValidNodes++);
            nodeArr[startPos.y][startPos.x] = thisNode;

            // Straight sections, weight 1: Right-Left, Up-Down sections.

            // Check right
            // Check for validity, and if so, either add connection or recurse and add later.
            if (startPos.x + 1 < numXNodes
                && isOkayToFloodUDLR(startPos, IntVec2.right, (Vector2)IntVec2.up))
            {
                floodDown(thisNode, startPos + IntVec2.right);
            }

            // Check left
            if (startPos.x - 1 >= 0
                && isOkayToFloodUDLR(startPos, IntVec2.left, (Vector2)IntVec2.up))
            {
                floodDown(thisNode, startPos + IntVec2.left);
            }

            // Check down (inverse coords!)
            if (startPos.y + 1 < numYNodes
                && isOkayToFloodUDLR(startPos, IntVec2.up, (Vector2)IntVec2.right))
            {
                floodDown(thisNode, startPos + IntVec2.up);
            }

            // Check up (inverse coords!)
            if (startPos.y - 1 >= 0
                && isOkayToFloodUDLR(startPos, IntVec2.down, (Vector2)IntVec2.right))
            {
                floodDown(thisNode, startPos + IntVec2.down);
            }

            // Check Diagonels: weighted at sqrt2.

            if (allowedPaths > Paths.quadDir)
            {
                //up-right
                // Check for validity, and if so, either add connection or recurse and add later.
                if (startPos.x + 1 < numXNodes
                    && startPos.y - 1 >= 0
                    && isOkayToFloodDiag(startPos, IntVec2.down, IntVec2.right))
                {
                    floodDown(thisNode, startPos + IntVec2.down_right, sqrt2);
                }

                //up-left
                if (startPos.x - 1 >= 0
                    && startPos.y - 1 >= 0
                    && isOkayToFloodDiag(startPos, IntVec2.down, IntVec2.left))
                {
                    floodDown(thisNode, startPos + IntVec2.down_left, sqrt2);
                }

                //down-right
                if (startPos.x + 1 < numXNodes
                    && startPos.y + 1 < numYNodes
                    && isOkayToFloodDiag(startPos, IntVec2.up, IntVec2.right))
                {
                    floodDown(thisNode, startPos + IntVec2.up_right, sqrt2);
                }

                //down-left
                if (startPos.x - 1 >= 0
                    && startPos.y + 1 < numYNodes
                    && isOkayToFloodDiag(startPos, IntVec2.up, IntVec2.left))
                {
                    floodDown(thisNode, startPos + IntVec2.up_left, sqrt2);
                }
            }
        }

        /// <summary>
        /// Returns whether a given position is free or not.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>True, if free, false if occupied</returns>
        private bool isFree(IntVec2 pos)
        {
            return nodeArr[pos.y][pos.x] == null;
        }

        /// <summary>
        /// Will flood to a position if possible,
        /// or add a connection to it if already made.
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="recurseTo"></param>
        /// <param name="weight"></param>
        private void floodDown(Node curNode, IntVec2 recurseTo, float weight = 1.0f)
        {
            if (isFree(recurseTo))
            {
                floodFill(recurseTo);
                curNode.addEdge(nodeArr[recurseTo.y][recurseTo.x], weight);
            }
            else
            {
                curNode.addEdge(nodeArr[recurseTo.y][recurseTo.x], weight);
            }
        }

        /// <summary>
        /// Helper function for floodfill. Does extra checks / physics checks to see if we can add a node in the specified location.
        /// </summary>
        /// <param name="startPos">The position to start from.</param>
        /// <param name="direction">The direction to go to.</param>
        /// <param name="orthAngle">The orthogonal to the direction.</param>
        /// <returns></returns>
        private bool isOkayToFloodUDLR(IntVec2 startPos, IntVec2 direction, Vector2 orthAngle)
        {
            IntVec2 newPos = startPos + direction;
            Vector2 newPosV2 = ArrPosToWorldSpace(newPos);
            Vector2 oldPosV2 = ArrPosToWorldSpace(startPos);

            //Linecase uses width in consideration of spreading to new nodes.
            return (!Physics2D.OverlapCircle(newPosV2, radii, LAYER_FILTER_MASK)
                 && !Physics2D.Linecast(oldPosV2 + orthAngle * radii, newPosV2 + orthAngle * radii, LAYER_FILTER_MASK)
                 && !Physics2D.Linecast(oldPosV2 - orthAngle * radii, newPosV2 - orthAngle * radii, LAYER_FILTER_MASK));
        }

        // Same, but with diagonals.
        private bool isOkayToFloodDiag(IntVec2 startPos, IntVec2 dir1, IntVec2 dir2)
        {
            IntVec2 newPos = startPos + dir1 + dir2;
            IntVec2 orth = dir1 - dir2;
            Vector2 orthAngle = ((Vector2) orth) / sqrt2;
            Vector2 newPosV2 = ArrPosToWorldSpace(newPos);
            Vector2 oldPosV2 = ArrPosToWorldSpace(startPos);

            //Linecase uses width in consideration of spreading to new nodes, this time diagonally
            return (!Physics2D.OverlapCircle(newPosV2, radii, LAYER_FILTER_MASK)
                 && !Physics2D.Linecast(oldPosV2 + orthAngle * radii, newPosV2 + orthAngle * radii, LAYER_FILTER_MASK)
                 && !Physics2D.Linecast(oldPosV2 - orthAngle * radii, newPosV2 - orthAngle * radii, LAYER_FILTER_MASK));
        }

        /// <summary>
        /// Converts a world space location to an array position.
        /// </summary>
        /// <param name="pos">The position in world space.</param>
        /// <returns>The position in array space.</returns>
        private IntVec2 WorldSpaceToArrPos(Vector2 pos)
        {
            float x = pos.x - lowerLeftBound.x;
            float y = pos.y - lowerLeftBound.y;
            x *= nodeDensity;
            y *= nodeDensity;

            return new IntVec2(x, y);
        }

        /// <summary>
        /// Converts an array position to world space.
        /// </summary>
        /// <param name="pos">The position in array space.</param>
        /// <returns>The position in world space.</returns>
        private Vector2 ArrPosToWorldSpace(IntVec2 pos)
        {
            float x = pos.x / nodeDensity;
            float y = pos.y / nodeDensity;
            x += lowerLeftBound.x;
            y += lowerLeftBound.y;

            return new Vector2(x, y);
        }
    }
}