using System;
using System.Collections.Generic;
using System.Text;

namespace Graph
{
    /// <summary>
    /// Manages entire sets of paths available.
    /// </summary>
    class PathManager
    {
        PathFinder pathFinderRef;

        class PathMemoizer
        {
            // Nodes we can walk to = squares we can get to this turn.
            private List<Node> nodesCanWalkTo;
            private HashSet<Node> nodesCanWalkToSet;

            // Nodes in range = all (unoccupied/occupied) squares we can shoot at this turn.
            private List<Node> nodesInRange;
            private HashSet<Node> nodesInRangeSet;

            // Reverse nodes in range = given something we can shoot at, give the 
            // square that uses the least amount of endurance to get to before shooting it.
            // This lets us max the damage.
            private Dictionary<Node, int> reverseNodesInRangeSetCost;
            
            // Current targets = All enemies we can shoot from this turn, paired with the square they can shoot at.
            private List<Node.NodePointer> currentTargets;


            private PathFinder pathFinderRef;
            private readonly Unit uRef;

            public PathMemoizer(PathFinder p, Unit unitRef)
            {
                uRef = unitRef;
                pathFinderRef = p;

                nodesCanWalkTo = new List<Node>();
                nodesCanWalkToSet = new HashSet<Node>();
                nodesInRange = new List<Node>(); ;
                nodesInRangeSet = new HashSet<Node>();
                reverseNodesInRangeSetCost = new Dictionary<Node, int>();
                currentTargets = new List<Node.NodePointer>();

                initialize(unitRef.getNode(), unitRef.getCurrentWater());
            }

            private void initialize(Node start, float endurance)
            {
                pathFinderRef.nodesThatSatisfyPred(start, pathfindPredicate, endurance);
                foreach (Node n in nodesCanWalkTo)
                {
                    shootingFrom = n;
                    pathFinderRef.runFuncOnAllNodesInRangeOfNode(shootingFrom, uRef.getMinAttackRange(), uRef.getMaxAttackRange(), rangeFunction);
                }
            }

            public int minCostToAttackSquare(Node square)
            {
                return reverseNodesInRangeSetCost[square];
            }

            // THIS IS A CHEAT: We only look at the node when we add to the done set.
            // We only add to the done set when this is called...
            // We can thus look at each node once, run what we need to run, and do this efficiently for each set.
            // See descriptions for each set above.
            private bool pathfindPredicate(Node lookAt)
            {
                nodesCanWalkTo.Add(lookAt);
                nodesCanWalkToSet.Add(lookAt);
                return true;
            }

            // temp node to allow rangeFunction to know where we're shooting from.
            private Node shootingFrom;

            private void rangeFunction(Node lookAt)
            {
                if (lookAt.isWalkable())
                {
                    // adds node to range pairs if not present already.
                    if (!nodesInRangeSet.Contains(lookAt))
                    {
                        nodesInRangeSet.Add(lookAt);
                        nodesInRange.Add(lookAt);
                    }

                    bool frendType = uRef.isEnemy();

                    // targets!
                    if (lookAt.Occupied && lookAt.Occupier.isEnemy() != frendType)
                    {
                        currentTargets.Add(new Node.NodePointer(shootingFrom, lookAt, Node.range(shootingFrom, lookAt)));
                    }

                    if (reverseNodesInRangeSetCost.ContainsKey(lookAt))
                    {
                        if (reverseNodesInRangeSetCost[lookAt] > shootingFrom.realCost)
                            reverseNodesInRangeSetCost[lookAt] = (int) shootingFrom.realCost;
                    }
                    else
                        reverseNodesInRangeSetCost.Add(lookAt, (int) (shootingFrom.realCost));
                }
            }

            internal List<Node> getAccessibleNodes(Unit u)
            {
                return nodesCanWalkTo;
            }

            internal List<Node> getNodesInRange(Unit u)
            {
                return nodesInRange;
            }

            internal List<Node.NodePointer> getCurrentTargets(Unit u)
            {
                return currentTargets;
            }

            internal bool canAttack(Node toAttack)
            {
                return nodesInRangeSet.Contains(toAttack);
            }

            internal HashSet<Node> getAccessibleNodesSet()
            {
                return nodesCanWalkToSet;
            }
        }

        private PathMemoizer unitRef;
        private Dictionary<Unit, PathMemoizer> enemyMemoizer;
            

        public PathManager(PathFinder p)
        {
            pathFinderRef = p;
            enemyMemoizer = new Dictionary<Unit, PathMemoizer>();
        }
        
        Unit currentUnit;

        /// <summary>
        /// Calculates a unit's paths all at once.
        /// </summary>
        /// <param name="u">The unit to memoize paths for.</param>
        public void calcUnitPaths(Unit u, List<Unit> enemies)
        {
            unitRef = new PathMemoizer(pathFinderRef, u);
            currentUnit = u;
            enemyMemoizer.Clear();

            foreach (Unit e in enemies)
            {
                enemyMemoizer.Add(e, new PathMemoizer(pathFinderRef, e));
            }
        }

        internal bool canAttack(Unit u, Node candidateMove)
        {
            if (u.Equals(currentUnit))
                return unitRef.canAttack(candidateMove);
            return enemyMemoizer[u].canAttack(candidateMove);
        }

        /// <summary>
        /// Get the currently accessible nodes for a given unit.
        /// </summary>
        /// <param name="u">Unit to query.</param>
        /// <returns>The list of all accessible nodes.</returns>
        public List<Node> getAccessibleNodes(Unit u)
        {
            if (u.Equals(currentUnit))
                return unitRef.getAccessibleNodes(u);
            return enemyMemoizer[u].getAccessibleNodes(u);
        }

        /// <summary>
        /// Get the currently accessible targets for a given unit.
        /// </summary>
        /// <param name="u">Unit to query.</param>
        /// <returns>The list of all positions where we can attack an enemy unit from.</returns>
        public List<Node.NodePointer> getCurrentTargets(Unit u)
        {
            if (u.Equals(currentUnit))
                return unitRef.getCurrentTargets(u);
            return enemyMemoizer[u].getCurrentTargets(u);
        }

        /// <summary>
        /// Returns the move cost of the maximum damage move (closest move that can attack a square, actually).
        /// </summary>
        internal int maxDamageMoveCost(Unit u, Node node)
        {
            if (u.Equals(currentUnit))
                return unitRef.minCostToAttackSquare(node);
            return enemyMemoizer[u].minCostToAttackSquare(node);
        }

        /// <summary>
        /// Get the current list of nodes in range for a given unit.
        /// </summary>
        /// <param name="u">Unit to query.</param>
        /// <returns>The list of all positions in range of unit.</returns>
        public List<Node> getNodesInRange(Unit u)
        {
            if (u.Equals(currentUnit))
                return unitRef.getNodesInRange(u);
            return enemyMemoizer[u].getNodesInRange(u);
        }

        /// <summary>
        /// Does Dijkstra to find the first thing that satisfies a pred, returns null if nothing found.
        /// </summary>
        public Node getClosestNode(Unit u, Predicate<Node> satisfies)
        {
            List<Node> nodeList = pathFinderRef.nodesThatSatisfyPred(u.getNode(), satisfies, float.PositiveInfinity, true, false);
            if (nodeList.Count > 0)
                return nodeList[0];
            else
                return null;
        }

        /// <summary>
        /// Returns true if a unit can walk to the given node with current endurance values.
        /// </summary>
        internal bool canWalkTo(Unit u, Node node)
        {
            if (u.Equals(currentUnit))
                return unitRef.getAccessibleNodesSet().Contains(node);
            return enemyMemoizer[u].getAccessibleNodesSet().Contains(node);
        }
    }
}
