using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCRouteService : MonoBehaviour
    {
        [SerializeField] private bool _isWaysVisible;
        [SerializeField] private bool _isAttractionPointsVisible;
        [SerializeField] private bool _isRootPointsVisible;

        [SerializeField] private List<NPCWay> _activeWays = new();

        // AttractionPoint.Id, ways 
        private Dictionary<string, List<NPCWay>> _confirmedWays = new();
        private List<NPCAttractionPoint> _allAttractionPoints;
        private List<NPCAttractionPoint> AllAttractionPoints
        {
            get
            {
                if (!_isInited)
                    BuildIndex();

                return _allAttractionPoints;
            }
            set => _allAttractionPoints = value;
        }

        private bool _isInited = false;

        private void Awake()
        {
            BuildIndex();
        }

        public Queue<NPCRootPoint> GetRoute(string startId, string endId)
        {
            List<NPCWay> ways = FindShortestRoute(startId, endId);

            if (ways.Count == 0)
                throw new ArgumentOutOfRangeException();

            Queue<NPCRootPoint> queue = new();

            foreach (NPCWay way in ways)
            {
                foreach (NPCRootPoint point in way.GetAllPoints())
                {
                    queue.Enqueue(point);
                }
            }

            return queue;
        }

        public Queue<NPCRootPoint> GetRandomRouteFrom(string startId)
        {
            List<NPCAttractionPoint> availablePoints = AllAttractionPoints.Where(point => point.Id != startId).ToList();
            string endId = availablePoints[Random.Range(0, availablePoints.Count)].Id;

            return GetRoute(startId, endId);
        }

        public NPCAttractionPoint GetNearstAttractionPoint(Vector3 targetPosition)
        {
            NPCAttractionPoint nearestPoint = null;
            float nearestDistance = float.MaxValue;

            foreach (NPCAttractionPoint point in AllAttractionPoints)
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

        private void BuildIndex()
        {
            _confirmedWays.Clear();

            foreach (NPCWay way in _activeWays)
            {
                if (way.From == null || way.To == null)
                    continue;

                if (!_allAttractionPoints.Contains(way.From))
                    _allAttractionPoints.Add(way.From);

                if (!_allAttractionPoints.Contains(way.To))
                    _allAttractionPoints.Add(way.To);

                AddWayToIndex(way);

                NPCWay reverseWay = way.CreateReverse();
                AddWayToIndex(reverseWay);
            }

            _isInited = true;
        }

        private void AddWayToIndex(NPCWay way)
        {
            string fromId = way.From.Id;
            string toId = way.To.Id;

            if (!_confirmedWays.ContainsKey(fromId))
            {
                _confirmedWays[fromId] = new List<NPCWay>();
            }

            if (!_confirmedWays[fromId].Any(existingWay => existingWay.To.Id == toId))
            {
                _confirmedWays[fromId].Add(way);
            }
        }

        private List<NPCWay> FindShortestRoute(string startId, string endId)
        {
            if (!_confirmedWays.ContainsKey(startId) || 
                !_confirmedWays.ContainsKey(endId) || 
                startId == endId)
                return new List<NPCWay>();

            Dictionary<string, float> distances = new();
            Dictionary<string, NPCWay> previousWay = new();
            Dictionary<string, string> previousNode = new();
            HashSet<string> unvisited = new();

            foreach (KeyValuePair<string, List<NPCWay>> kvp in _confirmedWays)
            {
                distances[kvp.Key] = float.MaxValue;
                unvisited.Add(kvp.Key);
            }

            distances[startId] = 0;

            while (unvisited.Count > 0)
            {
                string current = unvisited.OrderBy(id => distances[id]).First();

                if (current == endId)
                    break;

                unvisited.Remove(current);

                if (!_confirmedWays.ContainsKey(current))
                    continue;

                foreach (NPCWay way in _confirmedWays[current])
                {
                    string next = way.To.Id;

                    if (!unvisited.Contains(next))
                        continue;

                    float newDist = distances[current] + way.Distance;

                    if (newDist < distances[next])
                    {
                        distances[next] = newDist;
                        previousWay[next] = way;
                        previousNode[next] = current;
                    }
                }
            }

            List<NPCWay> route = new List<NPCWay>();
            string node = endId;

            while (previousWay.ContainsKey(node))
            {
                NPCWay way = previousWay[node];
                route.Insert(0, way);
                node = previousNode[node];
            }

            return route;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            foreach (NPCWay way in _activeWays)
            {
                if (way == null)
                    continue;

                if (_isWaysVisible)
                    way.DrawGizmos();

                if (way.From != null)
                    way.From.IsVisible = _isAttractionPointsVisible;

                if (way.To != null)
                    way.To.IsVisible = _isAttractionPointsVisible;

                foreach (NPCRootPoint rootPoint in way.Path)
                    if (way != null & rootPoint != null)
                        rootPoint.IsVisible = _isRootPointsVisible;
            }
        }
#endif
    }
}