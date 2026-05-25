using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    [Serializable]
    public class NPCWay
    {
        [field: SerializeField] public bool IsVisible {  get; set; }

        public NPCAttractionPoint From;
        public NPCAttractionPoint To;
        public List<NPCRoutePoint> Path = new();

        private Vector3 _cachedFromPosition;
        private Vector3 _cachedToPosition;
        private List<Vector3> _cachedPathPositions = new();
        private float _cachedDistance = -1;

        public float Distance
        {
            get
            {
                if (_cachedDistance < 0)
                    UpdateCaches();

                return _cachedDistance;
            }
        }

        public void UpdateCaches()
        {
            if (From == null && To == null)
            {
                Debug.LogError("Main points of the route are unknown");
                return;
            }

            _cachedFromPosition = From.Position;
            _cachedToPosition = To.Position;

            _cachedPathPositions.Clear();
            _cachedPathPositions.Capacity = Path.Count;

            foreach (NPCRoutePoint point in Path)
            {
                if (point != null)
                    _cachedPathPositions.Add(point.Position);
                else
                    _cachedPathPositions.Add(Vector3.zero);
            }

            CacheDistance();
        }

        public IEnumerable<NPCRoutePoint> GetAllPoints()
        {
            ExceptionUtilities.ThrowIfNull(From, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCWay", "From"));
            ExceptionUtilities.ThrowIfNull(To, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCWay", "To"));

            yield return From;

            foreach (NPCRoutePoint point in Path)
            {
                if (point != null)
                {
                    yield return point;
                }
            }

            yield return To;
        }

        public NPCWay CreateReverse()
        {
            List<NPCRoutePoint> reversedPath = new(Path);
            reversedPath.Reverse();

            return new NPCWay
            {
                From = this.To,
                To = this.From,
                Path = reversedPath
            };
        }

        private void CacheDistance()
        {
            _cachedDistance = 0;
            Vector3 prev = _cachedFromPosition;

            foreach (Vector3 point in _cachedPathPositions)
            {
                if (point != null && point != Vector3.zero)
                {
                    _cachedDistance += Vector3.Distance(prev, point);
                    prev = point;
                }
            }

            _cachedDistance += Vector3.Distance(prev, _cachedToPosition);
        }

#if UNITY_EDITOR
        private const float LINE_WIDTH = 2f;
        private readonly Color WAY_LINE_COLOR = Color.blue;

        public void DrawGizmos()
        {
            if (!IsVisible || From == null || To == null)
                return;

            Vector3 prev = _cachedFromPosition;

            foreach (Vector3 pointPos in _cachedPathPositions)
            {
                if (pointPos == Vector3.zero) 
                    continue;

                Vector3 current = pointPos;
                UnityEditor.Handles.DrawBezier(prev, current, prev, current, WAY_LINE_COLOR, null, LINE_WIDTH);
                prev = current;
            }

            Vector3 finalPos = _cachedToPosition;
            UnityEditor.Handles.DrawBezier(prev, finalPos, prev, finalPos, WAY_LINE_COLOR, null, LINE_WIDTH);
        }
#endif
    }
}