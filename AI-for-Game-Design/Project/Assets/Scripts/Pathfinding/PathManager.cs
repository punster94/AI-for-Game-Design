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

        private class Team
        {
            public Dictionary<Unit, List<Node>> NodesCanWalkTo;
            public Dictionary<Unit, List<Node>> NodesInRange;
            public Dictionary<Unit, HashSet<Node>> NodesInRangeSet;
            public Dictionary<Unit, List<Node.NodePointer>> CurrentTargets;

            public Team()
            {
                NodesCanWalkTo = new Dictionary<Unit, List<Node>>();
                NodesInRange = new Dictionary<Unit, List<Node>>();
                NodesInRangeSet = new Dictionary<Unit, HashSet<Node>>();
                CurrentTargets = new Dictionary<Unit, List<Node.NodePointer>>();
            }

            public void clear()
            {
                NodesCanWalkTo.Clear();
                CurrentTargets.Clear();
                NodesInRange.Clear();
                NodesInRangeSet.Clear();
            }
        }

        public PathManager(PathFinder p)
        {
            pathFinderRef = p;

            team1 = new Team();
            team2 = new Team();
        }

        Team team1, team2;

        private void clearAllPaths()
        {
            team1.clear();
            team2.clear();
        }

        bool team1Type;

        /// <summary>
        /// Re-sets the paths for a new turn.
        /// </summary>
        /// <param name="team1Units">Team 1: could be enemy or friend, as long as team1 != team2</param>
        /// <param name="team2Units">Team 2: could be enemy or friend, as long as team1 != team2</param>
        public void reserveNewPaths(List<Unit> team1Units, List<Unit> team2Units)
        {
            clearAllPaths();

            if (team1Units.Count == 0 && team2Units.Count == 0)
                return;
            if (team1Units.Count == 0)
                team1Type = !team2Units[0].isEnemy();
            else
                team1Type = team1Units[0].isEnemy();

            foreach (Unit u in team1Units)
            {
                team1.NodesCanWalkTo.Add(u, pathFinderRef.nodesWithinEnduranceValue(u.getNode(), u.getCurrentWater()));
                team1.NodesInRange.Add(u, pathFinderRef.NodesInRangeOfNodes(team1.NodesCanWalkTo[u], u.getMinAttackRange(), u.getMinAttackRange()));
                team1.CurrentTargets.Add(u, pathFinderRef.Targets(team1.NodesCanWalkTo[u], u.getMinAttackRange(), u.getMinAttackRange(), team1Type));
            }
            foreach (Unit u in team2Units)
            {
                team2.NodesCanWalkTo.Add(u, pathFinderRef.nodesWithinEnduranceValue(u.getNode(), u.getCurrentWater()));
                team2.NodesInRange.Add(u, pathFinderRef.NodesInRangeOfNodes(team1.NodesCanWalkTo[u], u.getMinAttackRange(), u.getMinAttackRange()));
                team2.CurrentTargets.Add(u, pathFinderRef.Targets(team2.NodesCanWalkTo[u], u.getMinAttackRange(), u.getMinAttackRange(), !team1Type));
            }
        }

        /// <summary>
        /// Get the currently accessible nodes for a given unit.
        /// </summary>
        /// <param name="u">Unit to query.</param>
        /// <returns>The list of all accessible nodes.</returns>
        public List<Node> getAccessibleNodes(Unit u)
        {
            if (u.isEnemy() == team1Type)
                return team1.NodesCanWalkTo[u];
            else
                return team2.NodesCanWalkTo[u];
        }

        /// <summary>
        /// Get the currently accessible targets for a given unit.
        /// </summary>
        /// <param name="u">Unit to query.</param>
        /// <returns>The list of all positions where we can attack an enemy unit from.</returns>
        public List<Node.NodePointer> getCurrentTargets(Unit u)
        {
            if (u.isEnemy() == team1Type)
                return team1.CurrentTargets[u];
            else
                return team2.CurrentTargets[u];
        }

        /// <summary>
        /// Get the current list of nodes in range for a given unit.
        /// </summary>
        /// <param name="u">Unit to query.</param>
        /// <returns>The list of all positions in range of unit.</returns>
        public List<Node> getNodesInRange(Unit u)
        {
            if (u.isEnemy() == team1Type)
                return team1.NodesInRange[u];
            else
                return team2.NodesInRange[u];
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
    }
}
