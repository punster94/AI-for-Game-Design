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

            public PathMemoizer(Unit unitRef)
            {
                uRef = unitRef;

                nodesCanWalkTo = new List<Node>();
                nodesInRange = new List<Node>(); ;
                nodesInRangeSet = new HashSet<Node>();
                currentTargets = new List<Node.NodePointer>();

                //TODO: Add logic here.
            }

            internal List<Node> getAccessibleNodes(Unit u)
            {
                throw new NotImplementedException();
            }

            internal List<Node> getNodesInRange(Unit u)
            {
                throw new NotImplementedException();
            }

            internal List<Node.NodePointer> getCurrentTargets(Unit u)
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
            unitRef = new PathMemoizer(u);
            enemyMemoizer.Clear();

            foreach (Unit e in enemies)
            {
                enemyMemoizer.Add(e, new PathMemoizer(e));
            }
        }

        internal bool canAttack(Unit enemy, Node candidateMove)
        {
            throw new NotImplementedException();
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

        internal int maxDamageMoveCost(Unit enemy, Node node)
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

        public Node getClosestNode(Unit u, Predicate<Node> satisfies)
        {
            List<Node> nodeList = pathFinderRef.nodesThatSatisfyPred(u.getNode(), satisfies, float.PositiveInfinity, true);
            if (nodeList.Count > 0)
                return nodeList[0];
            else
                return null;
        }

        /// <summary>
        /// Get the current list of nodes in range for a given unit.
        /// </summary>
        /// <param name="u">Unit to query.</param>
        /// <returns>The set of all positions attackable.</returns>
        public HashSet<Node> getNodesInRangeSet(Unit u)
        {
            throw new NotImplementedException("getNodesInRangeSet");
            /*
            if (u.isEnemy() == team1Type)
                return team1.NodesInRangeSet[u];
            else
                return team2.NodesInRangeSet[u];*/
        }

        internal bool inPaths(Unit subjectRef, Node node)
        {
            throw new NotImplementedException();
        }
    }
}
