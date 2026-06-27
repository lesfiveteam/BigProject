using Assets.Project.Scripts.NPC.NPCWalkSystem.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCRouteService : MonoBehaviour
    {
        public enum AlgorithmType
        {
            DFS,
            IDDFS,
            BIDDFS,
            BFS,
            BiBFS,
            Dijkstra,
            BiDijkstra,
            AStar,
            BiAStar,
        }

        [SerializeField] private bool _isDebug = false;

        [SerializeField] private NPCAttractionPoint _testPoint;

        [SerializeField] private AlgorithmType _algorithmType = AlgorithmType.BFS;

        [field: SerializeField] public List<NPCWay> ActiveWays { get; private set; } = new();
        private NPCRouteGraph _graph;

        private void Awake()
        {
            PathfindingAlgorithm currentAlgorithm = _algorithmType switch
            {
                AlgorithmType.DFS => new DFS(),
                AlgorithmType.IDDFS => new IDDFS(),
                AlgorithmType.BIDDFS => new BIDDFS(),
                AlgorithmType.BFS => new BFS(),
                AlgorithmType.BiBFS => new BIBFS(),
                AlgorithmType.Dijkstra => new Dijkstra(),
                AlgorithmType.BiDijkstra => new BIDijkstra(),
                AlgorithmType.AStar => new AStar(),
                AlgorithmType.BiAStar => new BIAStar(),
                _ => throw new ArgumentOutOfRangeException(nameof(_algorithmType), _algorithmType, null)
            };

            _graph = new NPCRouteGraph();
            _graph.Init(currentAlgorithm, ActiveWays, _isDebug);
        }

        public NPCAttractionPoint GetNearstAttractionPoint(Vector3 targetPosition)
        {
            NPCAttractionPoint nearestPoint = null;
            float nearestDistance = float.MaxValue;

            foreach (NPCAttractionPoint point in _graph.Vertices)
            {
                float pointDistance = Vector3.Distance(targetPosition, point.Position);

                if (pointDistance < nearestDistance)
                {
                    nearestDistance = pointDistance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }

        public Queue<NPCRoutePoint> GetRandomRouteFrom(NPCAttractionPoint startPoint)
        {
            HashSet<NPCAttractionPoint> availablePoints = new(_graph.Vertices.Where(point => point != startPoint));
            NPCAttractionPoint endPoint;

            if (availablePoints.Count == 0)
                Debug.LogError("Empty collection: availablePoints");

            if (availablePoints.Count == 1)
            {
                endPoint = availablePoints.First();
            }
            else
            {
                endPoint = GetRandomWithWeight(availablePoints, startPoint);
            }

            return GetRoute(startPoint, endPoint);
        }

        private NPCAttractionPoint GetRandomWithWeight(HashSet<NPCAttractionPoint> targetPoints, NPCAttractionPoint startPoint)
        {
            int totalWeight = startPoint.GraphWeightWithoutThis;

            float random = Random.Range(0f, totalWeight);
            float accumulate = 0f;

            foreach (NPCAttractionPoint point in targetPoints)
            {
                accumulate += point.Weight;

                if (random <= accumulate) 
                    return point;
            }

            return null;
        }

        private Queue<NPCRoutePoint> GetRoute(NPCAttractionPoint startPoint, NPCAttractionPoint endPoint)
        {
            List<NPCWay> ways;

            if (_isDebug)
            {
                endPoint = _testPoint;
            }
            
            if (ActiveWays.Count == 1)
            {
                ways = ActiveWays;
            }
            else
            {
                ways = _graph.GetShortestRoute(startPoint, endPoint);
            }

            if (ways.Count == 0)
                throw new ArgumentOutOfRangeException();

            Queue<NPCRoutePoint> queue = new();

            foreach (NPCWay way in ways)
            {
                foreach (NPCRoutePoint point in way.GetAllPoints())
                {
                    queue.Enqueue(point);
                }
            }

            return queue;
        }
    }
}