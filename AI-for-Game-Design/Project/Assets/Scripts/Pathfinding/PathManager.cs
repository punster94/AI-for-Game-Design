using UnityEngine;
using System.Collections;

namespace Graph
{
    public class PathManager : MonoBehaviour {
        private static PathFinder denseGraph;

        public static readonly string PathManagerTag = "Path Manager";

        private static GameObject self;

        public void Start()
        {
            if (self == null)
            {
                self = new GameObject(PathManagerTag);
            }
            if (denseGraph == null)
            {
                denseGraph = self.AddComponent<PathFinder>();
                denseGraph.initializeGraph(new Vector2(-17, -13), new Vector2(17, 14), 1.0f, 0.75f);
            }
        }

        public static PathFinder getDenseGraph()
        {
            if (self == null)
            {
                self = new GameObject(PathManagerTag);
            }
            if (denseGraph == null)
            {
                denseGraph = self.AddComponent<PathFinder>();
                denseGraph.initializeGraph(new Vector2(-17, -13), new Vector2(17, 14), 1.0f, 0.75f);
            }
            return denseGraph;
        }
    }
}
