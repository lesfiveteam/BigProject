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

        private IAlgorithm _algorithm;

        private int _totalWeight;
        private bool _isInited = false;

        public HashSet<NPCAttractionPoint> Vertices {  get; private set; } = new();

        public void Init(IAlgorithm algorithm, List<NPCWay> edges)
        {
            _algorithm = algorithm;

            BuildIndex(edges);
            InitVertices();

            _isInited = true;
        }

        public List<NPCWay> GetShortestRoute(NPCAttractionPoint startVertex, NPCAttractionPoint endVertex)
        {
            if (!_isInited)
                Debug.LogError("NPCRouteGraph is not inited!");

            // for perf test
            //#if UNITY_EDITOR
            //            System.Diagnostics.Stopwatch sw = new();
            //            sw.Start();
            //#endif

            List<NPCWay> route = _algorithm.FindShortestWay(_adjList, startVertex, endVertex);

            //#if UNITY_EDITOR
            //            sw.Stop();
            //            Debug.Log($"Выполнение заняло: {sw.ElapsedTicks} ticks");
            //            Debug.Log($"В маршруте: {route.Count} точек");
            //            float routeLenght = 0f;
            //            foreach (NPCWay way in route)
            //            {
            //                routeLenght += Vector3.Distance(way.From.Position, way.To.Position);
            //                Debug.Log($"точка: {way.From.name}");
            //            }
            //            Debug.Log($"Длинна маршрута: {routeLenght}");
            //#endif

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