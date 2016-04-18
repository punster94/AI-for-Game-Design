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
            private List<Node> nodesCanWalkTo;
            private List<Node> nodesInRange;
            private HashSet<Node> nodesInRangeSet;
            private List<Node.NodePointer> currentTargets;
            private readonly Unit uRef;

            public PathMemoizer(PathFinder p, Unit unitRef)
            {
                uRef = unitRef;

                nodesCanWalkTo = new List<Node>();
                nodesInRange = new List<Node>(); ;
                nodesInRangeSet = new HashSet<Node>();
                currentTargets = new List<Node.NodePointer>();

                throw new NotImplementedException();
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

            internal HashSet<Node> getNodesInRangeSet()
            {
                return nodesInRangeSet;
            }

            internal HashSet<Node> getAccessibleNodesSet()
            {
                throw new NotImplementedException();
            }
        }

        private PathMemoizer unitRef;
        private Dictionary<Unit, PathMemoizer> enemyMemoizer;
            

        public PathManager(PathFinder p)
        {
            pathFinderRef = p;
        }


        bool team1Type;
        Unit currentUnit;

        /// <summary>
        /// Calculates a unit's paths all at once.
        /// </summary>
        /// <param name="u">The unit to memoize paths for.</param>
        public void calcUnitPaths(Unit u, List<Unit> enemies)
        {
            unitRef = new PathMemoizer(pathFinderRef, u);
            enemyMemoizer.Clear();

            foreach (Unit e in enemies)
            {
                enemyMemoizer.Add(e, new PathMemoizer(pathFinderRef, e));
            }
        }

        internal bool canAttack(Unit u, Node candidateMove)
        {
            if (u.Equals(currentUnit))
                return unitRef.getNodesInRangeSet().Contains(candidateMove);
            return enemyMemoizer[u].getNodesInRangeSet().Contains(candidateMove);
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
            throw new NotImplementedException();
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
            List<Node> nodeList = pathFinderRef.nodesThatSatisfyPred(u.getNode(), satisfies, float.PositiveInfinity, true);
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
