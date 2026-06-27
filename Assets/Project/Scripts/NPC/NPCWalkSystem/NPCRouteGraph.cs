/// Perf test for Villagers route system:
/// - from point:   1
/// - to point:     22
/// 
/// Results:
/// |   Algorithm   |  Points in route  |   ~Ticks      |   Comment
/// |--------------------------------------------------------------------------
/// |   BFS         |       12          |   900-1300    |   selected as default
/// |   DFS         |       16          |   800-1400    |   not optimal route
/// |   Dijkstra    |       12          |   3100-4500   |
/// |   AStar       |       12          |   1600-2000   |
/// |   BiAStar     |       12          |   3300-5500   |

using Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCRouteGraph
    {
        private Dictionary<NPCAttractionPoint, List<NPCWay>> _adjList = new();

        private PathfindingAlgorithm _algorithm;

        private int _totalWeight;
        private bool _isDebug = false;
        private bool _isInited = false;


        public HashSet<NPCAttractionPoint> Vertices {  get; private set; } = new();

        public void Init(PathfindingAlgorithm algorithm, List<NPCWay> edges, bool isDebug)
        {
            _algorithm = algorithm;
            _isDebug = isDebug;

            BuildIndex(edges);
            InitVertices();

            _isInited = true;
        }

        public List<NPCWay> GetShortestRoute(NPCAttractionPoint startVertex, NPCAttractionPoint endVertex)
        {
            if (!_isInited)
                Debug.LogError("NPCRouteGraph is not inited!");

            List<NPCWay> route = new();

            //for perf test
#if UNITY_EDITOR
            if (_isDebug)
            {
                System.Diagnostics.Stopwatch sw = new();
                sw.Start();

                route = _algorithm.FindShortestWay(_adjList, startVertex, endVertex);

                sw.Stop();
                Debug.Log($"Route calculation took: {sw.ElapsedTicks} ticks");
                Debug.Log($"Route has: {route.Count} egdes");
                float routeLenght = 0f;
                string routePoints = "Route has points: ";

                for (int i = 0; i < route.Count; i++)
                {
                    routeLenght += Vector3.Distance(route[i].From.Position, route[i].To.Position);
                    routePoints += route[i].From.name + ", ";

                    if (i == route.Count - 1)
                    {
                        routePoints += route[i].To.name;
                    }
                }

                Debug.Log(routePoints);
                Debug.Log($"Route length: {routeLenght}");
            }
#endif

            if (route.Count == 0)
                route = _algorithm.FindShortestWay(_adjList, startVertex, endVertex);

            return route;
        }

        private void BuildIndex(List<NPCWay> edges)
        {
            _adjList.Clear();

            foreach (NPCWay edge in edges)
            {
                if (edge.From == null || edge.To == null)
                    continue;

                AddWayToIndex(edge);

                NPCWay reverseEdge = edge.CreateReverse();
                AddWayToIndex(reverseEdge);
            }
        }

        private void AddWayToIndex(NPCWay edge)
        {
            NPCAttractionPoint fromVertex = edge.From;
            NPCAttractionPoint toVertex = edge.To;

            if (!_adjList.ContainsKey(fromVertex))
            {
                _adjList[fromVertex] = new List<NPCWay>();
                _totalWeight += fromVertex.Weight;
            }

            if (!_adjList[fromVertex].Any(existingVertex => existingVertex.To == toVertex))
            {
                _adjList[fromVertex].Add(edge);
                _ = edge.Distance;
            }
        }

        private void InitVertices()
        {
            foreach (NPCAttractionPoint vertex in _adjList.Keys)
            {
                int currentVertexWeightWithoutThis = _totalWeight - vertex.Weight;
                vertex.Init(currentVertexWeightWithoutThis);

                Vertices.Add(vertex);
            }
        }
    }
}