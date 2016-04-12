//#define DEBUG_PATHFINDER_UPDATELOOP // allows graph to auto-update w/physics, display manual paths.
//#define DEBUG_PATHFINDER_DRAWDEBUG  // draws debug paths and shows start/end nodes.
//#define DEBUG_PATHFINDER_LOGDEBUG   // sends pathfinding debug to debug. SIGNIFICANT PERFORMANCE IMPACT!
using UnityEngine;
using System.Collections.Generic;
using System.Text;

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

        /// TODO: Implement this in PCG, or use suggestion in MainGame
        /// <summary>
        /// Generates a pair of list of spawn points. Assumes graph already generated.
        /// Picks either top right, bottom left, or top left, bottom right pairs.
        /// Either key/value can be enemy/friend spawn points.
        /// Spawns with no gap, but could randomly select from returned positions.
        /// </summary>
        /// <param name="spawnPointsPerPlayer">Number of spawn points to provide each player</param>
        /// <returns>A pair of spawn points (lists of valid node points), empty if failed.</returns>
        public KeyValuePair<List<Node>, List<Node>> getSpawnPoints(int spawnPointsPerPlayer = 5)
        {
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
                        
            return getSpawnPoints(locusEnemy, locusFriend, spawnPointsPerPlayer);
        }


        /// TODO: Implement this in PCG, or use suggestion in MainGame
        /// <summary>
        /// Generates a pair of list of spawn points. Assumes graph already generated.
        /// Given a pair of locus points, tries to generate a spawn point near it.
        /// Either key/value can be enemy/friend spawn points.
        /// Spawns with no gap, but could randomly select from returned positions.
        /// </summary>
        /// <param name="spawnPointsPerPlayer">Number of spawn points to provide each player</param>
        /// <returns>A pair of spawn points (lists of valid node points), empty if failed.</returns>
        public KeyValuePair<List<Node>, List<Node>> getSpawnPoints(Vector2 spawnPoint1, Vector2 spawnPoint2, int spawnPointsPerPlayer = 5)
        {
            List<Node> enemy = new List<Node>(), friend = new List<Node>();

            //The midpoint vector of sorts. 
            Vector2 shiftVec = (spawnPoint2 - spawnPoint1) / 8;

            Node spawnEn = null, spawnFr = null;
            int maxTries = 10;

            // Tries to create a path
            for (int i = 0; i < maxTries; i++)
            {
                spawnEn = BFSUnoccupiedAndValid(spawnPoint1);
                spawnFr = BFSUnoccupiedAndValid(spawnPoint2);

                // Checks for AStar connection between spawns.
                Queue<Node> pathToSpawns = new Queue<Node>();
                AStar(pathToSpawns, spawnFr, spawnEn);

                // Failed: no connection.
                if (pathToSpawns.Count == 0)
                {
                    spawnEn = spawnFr = null;

                    // Takes a sorta random walk towards the midpoint.
                    // This duplicates the action taken on either side.
                    float randWalkToMidX = Random.value;
                    float randWalkToMidY = Random.value;
                    Vector2 shiftToCenter = new Vector2(shiftVec.x * randWalkToMidX, shiftVec.y * randWalkToMidY);

                    spawnPoint1 += shiftToCenter;
                    spawnPoint2 -= shiftToCenter;
                }
                else
                    break;
            }

            if (spawnEn == null)
            {
                throw new KeyNotFoundException("Didn't find a spawn location!");
            }

            // Reserves occupation, so we can loop through graph.
            spawnEn.Occupied = spawnFr.Occupied = true;
            enemy.Add(spawnEn);
            friend.Add(spawnFr);
            spawnPointsPerPlayer--;

            while (spawnPointsPerPlayer > 0)
            {
                Node foundEn, foundFr;

                foundEn = BFSUnoccupiedAndValid(spawnEn);
                foundFr = BFSUnoccupiedAndValid(spawnFr);

                // Error conditions for BFS
                // Could just break instead, and return less spawn points... bad idea.
                if (foundEn == spawnEn || foundFr == spawnFr)
                {
                    throw new KeyNotFoundException("Didn't find a spawn location!");
                }

                foundEn.Occupied = foundFr.Occupied = true;

                enemy.Add(foundEn);
                friend.Add(foundFr);

                spawnPointsPerPlayer--;
            }

            // Resets occupied status to allow future use.
            foreach (Node n in enemy)
                n.Occupied = false;
            foreach (Node n in friend)
                n.Occupied = false;

            return new KeyValuePair<List<Node>, List<Node>>(enemy, friend);
        }

        // Node array is stored as Node[inverse-y][x].
        private Node[][] nodeArr;
        private float radii;
        private Sprite nodeImg;

        //Reserves image before called upon.
        public void Awake()
        {
            nodeImg = Resources.Load<Sprite>("PathLoc");
        }

        private static int LAYER_FILTER_MASK = LayerMask.GetMask("Walls");

        public void initializeWithArray(Vector2 lowLeftBound, Vector2 upRightBound, Node.SquareType[][] terrainArr)
        {
            lowerLeftBound = lowLeftBound;
            upperRightBound = upRightBound;
            
            numXNodes = terrainArr[0].Length;
            numYNodes = terrainArr.Length;

            // Determine correct node density
            float xDist = lowerLeftBound.x - upperRightBound.x;
            nodeDensity = numXNodes / xDist;

            // Magic number allows for proper drawing of nodes.
            radii = 0.75f / nodeDensity;
            
            numValidNodes = 0;

            // Initialize the node array. Note the inverted y, normal x setup.
            nodeArr = new Node[numYNodes][];
            for (int y = 0; y < numYNodes; y++)
            {
                nodeArr[y] = new Node[numXNodes];

                for (int x = 0; x < numXNodes; x++)
                {
                    IntVec2 arrPos = new IntVec2(x, y);
                    Vector2 newPosV2 = ArrPosToWorldSpace(arrPos);

                    nodeArr[y][x] = new Node(transform.gameObject, nodeImg, newPosV2, arrPos, terrainArr[y][x], radii * 2, numValidNodes++);

                    //add up-left edge (inverse y)
                    if (allowedPaths > Paths.quadDir)
                    {
                        if (x - 1 >= 0 && y - 1 >= 0
                            && isOkayToFloodDiag(arrPos, IntVec2.down, IntVec2.left))
                            nodeArr[y][x].addBidirEdge(nodeArr[y - 1][x - 1], sqrt2);
                        //add up-right edge (inverse y)
                        if (x + 1 < numXNodes && y - 1 >= 0
                            && isOkayToFloodDiag(arrPos, IntVec2.down, IntVec2.right))
                            nodeArr[y][x].addBidirEdge(nodeArr[y - 1][x + 1], sqrt2);
                    }
                    //add up edge
                    if (y - 1 >= 0
                        && isOkayToFloodUDLR(arrPos, IntVec2.down, (Vector2)IntVec2.left))
                        nodeArr[y][x].addBidirEdge(nodeArr[y - 1][x], 1);
                    //add left edge
                    if (x - 1 >= 0
                        && isOkayToFloodUDLR(arrPos, IntVec2.left, (Vector2)IntVec2.up))
                        nodeArr[y][x].addBidirEdge(nodeArr[y][x - 1], 1);
                }
            }

            isInitialized = true;
        }

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

            numValidNodes = 0;

            //floodFill(startPos);
            fillAll();

            isInitialized = true;
        }

        ///Initializes nodes to ifinity realcost, heuristic, null camefrom, and not visited.
        private void initializePathfinding()
        {
            foreach (Node[] arr in nodeArr)
                foreach (Node p in arr)
                    if (p != null)
                    {
                        p.initPathfinding();
                    }
        }

        /// <summary>
        /// Returns a sorted list of nodes within a given endurance value.
        /// Performs a Dijkstra-like algorithm.
        /// </summary>
        /// <param name="satifies">The predicate each node must follow.</param>
        /// <param name="endurance">The maximum endurance to follow out.</param>
        /// <returns>A sorted list of nodes within a given endurance value.</returns>
        public List<Node> nodesThatSatisfyPred(Node startNode, System.Predicate<Node> satifies, float endurance = 16.0f)
        {
            List<Node> foundNodes = new List<Node>();
            MinPriorityQueue<Node> nodeList = new MinPriorityQueue<Node>();

            initializePathfinding();

            startNode.realCost = 0;
            nodeList.Enqueue(startNode, startNode.realCost);

#if DEBUG_PATHFINDER_LOGDEBUG
            StringBuilder encountered = new StringBuilder();
            StringBuilder nodes = new StringBuilder();
            nodes.Append("Start node ").Append(startNode.Number).AppendLine();
            encountered.Append("Start node ").Append(startNode.Number).AppendLine();
            encountered.Append("endurance = ").Append(endurance).AppendLine();
#endif

            while (nodeList.Count > 0)
            {
                //Pick the best looking node, by f-value.
                Node best = nodeList.Dequeue();
                double bestDist = best.realCost;
                
#if DEBUG_PATHFINDER_LOGDEBUG
                encountered.Append("Node ").Append(best.Number).Append(" ").Append(best).AppendLine();
                nodes.Append("Node ").Append(best.Number).AppendLine();
#endif

                best.Visited = true;

                if (satifies(best))
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

#if DEBUG_PATHFINDER_LOGDEBUG
                        encountered.Append("   added ").Append(other.Number)
                                   .Append(", total estimated cost ")
                                   .Append(other.realCost).AppendLine();
#endif
                        continue;
                    }
                    //If the other node was a bad path, and this one's better, replace it.
                    else if (other.realCost > testDist)
                    {
                        other.CameFrom = best;
                        other.realCost = testDist;
                        nodeList.Update(other, other.realCost);

#if DEBUG_PATHFINDER_LOGDEBUG
                        encountered.Append("   updated ").Append(other.Number)
                                   .Append(", total new estimated cost ")
                                   .Append(other.realCost).AppendLine();
#endif
                    }
                }
            }
#if DEBUG_PATHFINDER_LOGDEBUG
            Debug.Log(encountered);
            Debug.Log(nodes);
#endif

            return foundNodes;
        }

        /// <summary>
        /// Returns a sorted list of nodes within a given endurance value.
        /// Performs a Dijkstra-like algorithm.
        /// </summary>
        /// <param name="endurance">The maximum endurance to follow out.</param>
        /// <returns>A sorted list of nodes within a given endurance value.</returns>
        public List<Node> nodesWithinEnduranceValue(Node startNode, float endurance = 16.0f)
        {
            //for future reference, less code copy-paste worth negligible performance reduction.
            return nodesThatSatisfyPred(startNode, (_) => true, endurance);
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
        /// Unlike closestMostValidNode; BFS does up to a full BFS
        /// to find the closest unocupied & walkable node to a given location.
        /// Only use when not wanting to take into account edge costs.
        /// If no nodes found that are valid, returns the start node.
        /// </summary>
        /// <param name="startLoc">The location to start looking from.</param>
        /// <returns>The first unocupied/valid walkable tile found. If no nodes found that are valid, returns the start node.</returns>
        private Node BFSUnoccupiedAndValid(Vector2 startLoc)
        {
            Node startNode = closestMostValidNode(startLoc);
            return BFSUnoccupiedAndValid(startNode);
        }


        /// <summary>
        /// Unlike closestMostValidNode; BFS does up to a full BFS
        /// to find the closest unocupied & walkable node to a given location.
        /// Only use when not wanting to take into account edge costs.
        /// If no nodes found that are valid, returns the start node.
        /// Uses Nodes directly instead of converting from Vector2.
        /// </summary>
        /// <param name="startLoc">The location to start looking from.</param>
        /// <returns>The first unocupied/valid walkable tile found. If no nodes found that are valid, returns the start node.</returns>
        private Node BFSUnoccupiedAndValid(Node startNode)
        {
            if (!startNode.Occupied && startNode.isWalkable())
                return startNode;

            initializePathfinding();
            
            Queue<Node> listOfNodes = new Queue<Node>();
            listOfNodes.Enqueue(startNode);

            // Can't use visited, as we're already using that hack in initializePathfinding...
            startNode.realCost = -1;

            while (listOfNodes.Count > 0)
            {
                Node found = listOfNodes.Dequeue();
                if (!found.Occupied && found.isWalkable())
                    return found;

                foreach (Edge e in found.getEdges())
                {
                    Node candidate = e.getNode();
                    if (candidate.realCost > 0)
                        listOfNodes.Enqueue(candidate);

                    // Can't use visited, as we're already using that hack in initializePathfinding...
                    candidate.realCost = -1;
                }
            }

            return startNode;
        }
        
        /// <summary>
        /// Finds the closest most-valid node to a given position.
        /// In worst case runs line tests on all created nodes in graph.
        /// </summary>
        /// <param name="loc">The location to start looking from.</param>
        /// <returns>The most valid node that's closest to the given location.</returns>
        public Node closestMostValidNode(Vector2 loc)
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
            // Since no node is null now, we can remove the rest.

#if DEBUG_PATHFINDER_UPDATELOOP
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
#endif
            return closest;
        }

        private Node unobstructed(int x, int y, Vector2 pos)
        {
            bool failed = x < 0 || y < 0 || x >= numXNodes || y >= numYNodes || nodeArr[y][x] == null;
#if DEBUG_PATHFINDER_UPDATELOOP
            failed |= Physics2D.Linecast(nodeArr[y][x].getPos(), pos, LAYER_FILTER_MASK))
#endif
            if (failed)
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

            initializePathfinding();
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

#if DEBUG_PATHFINDER_LOGDEBUG
            StringBuilder encountered = new StringBuilder();
            StringBuilder nodes = new StringBuilder();
            nodes.Append("Start node ").Append(start.Number).AppendLine();
            encountered.Append("Start node ").Append(start.Number).AppendLine();
            nodes.Append("End node ").Append(end.Number).AppendLine();
            encountered.Append("End node ").Append(end.Number).AppendLine();
#endif

            while (nodeList.Count > 0)
            {
                //Pick the best looking node, by f-value.
                Node best = nodeList.Dequeue();
                double bestDist = best.realCost;

#if DEBUG_PATHFINDER_LOGDEBUG
                encountered.Append("Node ").Append(best.Number).Append(" ").Append(best).AppendLine();
                nodes.Append("Node ").Append(best.Number).AppendLine();
#endif

                //If this is the end, stop, show the path, and return it.
                if (best.Equals(end))
                {
                    ReconstructPath(pathStoreLoc, end);
                    ShowPath(pathStoreLoc);

#if DEBUG_PATHFINDER_LOGDEBUG
                    encountered.Append("Finished!\n\nFinal dist: ")
                               .Append(best.realCost).AppendLine();
                    Debug.Log(encountered);
                    Debug.Log(nodes);
#endif
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

#if DEBUG_PATHFINDER_LOGDEBUG
                        encountered.Append("   added ").Append(other.Number)
                                   .Append(", total estimated cost ")
                                   .Append(other.realCost + other.heuristicCost)
                                   .AppendLine();
#endif
                        continue;
                    }
                    //If the other node was a bad path, and this one's better, replace it.
                    else if (other.realCost > testDist)
                    {
                        other.CameFrom = best;
                        other.realCost = testDist;
                        nodeList.Update(other, other.realCost + other.heuristicCost);

#if DEBUG_PATHFINDER_LOGDEBUG
                        encountered.Append("   updated ").Append(other.Number)
                                   .Append(", total new estimated cost ")
                                   .Append(other.realCost + other.heuristicCost)
                                   .AppendLine();
#endif
                    }
                }
            }

#if DEBUG_PATHFINDER_LOGDEBUG
            encountered.Append("Failed!\n");
            Debug.Log(encountered);
            Debug.Log(nodes);
#endif
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

        public List<Node> NodesInRangeOfNodes(List<Node> listNodes, int minDist, int maxDist)
        {
            HashSet<Node> inRange = new HashSet<Node>();

            foreach (Node n in listNodes)
                nodesInRangeOfNode(inRange, n.getGridPos().x, n.getGridPos().y, minDist, maxDist);

            List<Node> listOfNodes = new List<Node>();
            listOfNodes.AddRange(inRange);
            return listOfNodes;
        }

        private void nodesInRangeOfNode(HashSet<Node> inRange, int x, int y, int minDist, int maxDist)
        {
            if (maxDist < 0 || x < 0 || x >= numXNodes || y < 0 || y >= numYNodes)
                return;

            if (minDist <= 0 && nodeArr[y][x].isWalkable())
                inRange.Add(nodeArr[y][x]);
            
            nodesInRangeOfNodeLeft(inRange, x - 1, y, minDist - 1, maxDist - 1);
            nodesInRangeOfNodeRight(inRange, x + 1, y, minDist - 1, maxDist - 1);
            nodesInRangeOfNodeUp(inRange, x, y - 1, minDist - 1, maxDist - 1);
            nodesInRangeOfNodeDown(inRange, x, y + 1, minDist - 1, maxDist - 1);
        }

        private void nodesInRangeOfNodeLeft(HashSet<Node> inRange, int x, int y, int minDist, int maxDist)
        {
            if (maxDist < 0 || x < 0 || x >= numXNodes || y < 0 || y >= numYNodes)
                return;

            if (minDist <= 0 && nodeArr[y][x].isWalkable())
                inRange.Add(nodeArr[y][x]);

            nodesInRangeOfNodeLeft(inRange, x - 1, y, minDist - 1, maxDist - 1);
        }


        private void nodesInRangeOfNodeRight(HashSet<Node> inRange, int x, int y, int minDist, int maxDist)
        {
            if (maxDist < 0 || x < 0 || x >= numXNodes || y < 0 || y >= numYNodes)
                return;

            if (minDist <= 0 && nodeArr[y][x].isWalkable())
                inRange.Add(nodeArr[y][x]);

            nodesInRangeOfNodeRight(inRange, x + 1, y, minDist - 1, maxDist - 1);
        }

        private void nodesInRangeOfNodeUp(HashSet<Node> inRange, int x, int y, int minDist, int maxDist)
        {
            if (maxDist < 0 || x < 0 || x >= numXNodes || y < 0 || y >= numYNodes)
                return;

            if (minDist <= 0 && nodeArr[y][x].isWalkable())
                inRange.Add(nodeArr[y][x]);

            nodesInRangeOfNodeLeft(inRange, x - 1, y, minDist - 1, maxDist - 1);
            nodesInRangeOfNodeRight(inRange, x + 1, y, minDist - 1, maxDist - 1);
            nodesInRangeOfNodeUp(inRange, x, y - 1, minDist - 1, maxDist - 1);
        }

        private void nodesInRangeOfNodeDown(HashSet<Node> inRange, int x, int y, int minDist, int maxDist)
        {
            if (maxDist < 0 || x < 0 || x >= numXNodes || y < 0 || y >= numYNodes)
                return;

            if (minDist <= 0 && nodeArr[y][x].isWalkable())
                inRange.Add(nodeArr[y][x]);

            nodesInRangeOfNodeLeft(inRange, x - 1, y, minDist - 1, maxDist - 1);
            nodesInRangeOfNodeRight(inRange, x + 1, y, minDist - 1, maxDist - 1);
            nodesInRangeOfNodeDown(inRange, x, y + 1, minDist - 1, maxDist - 1);
        }
        /// <summary>
        /// The graph has a update function, for showing off pathfinding
        /// without using the subject as a starting node.
        /// Can also automatically update the graph to changes if enabled.
        /// </summary>
        void Update()
        {
			
        }

		public void displayRangeOfUnit(Unit u, Vector2 mousePosition) {
			if (overlayNodes == null)
				overlayNodes = new List<Node>();

			foreach (Node n in overlayNodes)
				if (n != null)
					n.destroyThisNode();

			overlayNodes.Clear();

			List<Node> reach = nodesWithinEnduranceValue(closestMostValidNode(mousePosition), u.getCurrentWater());
			List<Node> range = NodesInRangeOfNodes(reach, u.getMinAttackRange(), u.getMaxAttackRange());

			HashSet<Node> inReach = new HashSet<Node>();
			inReach.UnionWith(reach);
			HashSet<Node> inRange = new HashSet<Node>();
			inRange.UnionWith(range);
			inRange.RemoveWhere(inReach.Contains);

			foreach (Node n in reach)
			{
				//make slightly smaller to show square off
				Node q = new Node(transform.gameObject, nodeImg, n.getPos(), n.getGridPos(), Node.randWalkState(), radii * 1.75f);
				q.setColor(new Color(0, 0.5f, 0, 0.75f));

				overlayNodes.Add(q);
			}


			foreach (Node n in inRange)
			{
				//make slightly smaller to show square off
				Node q = new Node(transform.gameObject, nodeImg, n.getPos(), n.getGridPos(), Node.randWalkState(), radii * 1.75f);
				q.setColor(new Color(0, 0, 0.5f, 0.75f));

				overlayNodes.Add(q);
			}
		}
        
		// TODO: Remove dependence on this since MainGame will deal with clicks
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
                manualStartNode.resetColor();
            if (manualEndNode != null)
                manualEndNode.resetColor();

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

#if DEBUG_PATHFINDER_UPDATELOOP
                    if (!Physics2D.OverlapCircle(newPosV2, radii, LAYER_FILTER_MASK))
#endif
                    {
                        nodeArr[y][x] = new Node(transform.gameObject, nodeImg, newPosV2, arrPos, Node.randWalkState(), radii * 2, numValidNodes++);

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

        private const float sqrt2 = 1.41421356237f;

#if DEBUG_PATHFINDER_UPDATELOOP
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
#else
        /// <summary>
        /// null helper function for floodfill.
        /// Returns true.
        /// </summary>
        /// <returns>true!</returns>
        private bool isOkayToFloodUDLR(IntVec2 startPos, IntVec2 direction, Vector2 orthAngle)
        {
            return true;
        }

        // Same, but with diagonals.
        private bool isOkayToFloodDiag(IntVec2 startPos, IntVec2 dir1, IntVec2 dir2)
        {
            return true;
        }
#endif

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