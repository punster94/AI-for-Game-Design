using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Graph
{
    /// <summary>
    /// Object used for binning a graph.
    /// </summary>
    public class GraphManager {
        private PathFinder graph;

        public readonly string PathManagerTag = "GraphManager";

        private GameObject self;

        public GraphManager(GameObject parent)
        {
            self = new GameObject(PathManagerTag);
            self.transform.parent = parent.transform;
            graph = self.AddComponent<PathFinder>();
            graph.initializeGraph(new Vector2(-26, -17), new Vector2(25, 15), 1.0f, 0.75f);
        }

	public GraphManager(GameObject parent, Node.SquareType[][] map)
	{
		self = new GameObject(PathManagerTag);
		self.transform.parent = parent.transform;
		graph = self.AddComponent<PathFinder>();
		graph.initializeWithArray(new Vector2(-26, -17), new Vector2(25, 15), map);
	}

		public PathFinder getGraph()
        {
            return graph;
        }
    }
}
