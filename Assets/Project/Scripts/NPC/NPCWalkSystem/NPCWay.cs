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
        public NPCAttractionPoint From;
        public NPCAttractionPoint To;
        public List<NPCRootPoint> Path = new();

        [NonSerialized] private float _cachedDistance = -1;

        public float Distance
        {
            get
            {
                if (_cachedDistance < 0)
                    CalculateDistance();

                return _cachedDistance;
            }
        }

        private void CalculateDistance()
        {
            _cachedDistance = 0;
            Vector3 prev = From.Position;

            foreach (NPCRootPoint point in Path)
            {
                if (point != null)
                {
                    _cachedDistance += Vector3.Distance(prev, point.Position);
                    prev = point.Position;
                }
            }

            if (To != null)
            {
                _cachedDistance += Vector3.Distance(prev, To.Position);
            }
        }

        public IEnumerable<NPCRootPoint> GetAllPoints()
        {
            ExceptionUtilities.ThrowIfNull(From, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCWay", "From"));
            ExceptionUtilities.ThrowIfNull(To, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"NPCWay", "To"));

            yield return From;

            foreach (NPCRootPoint point in Path)
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
            List<NPCRootPoint> reversedPath = new(Path);
            reversedPath.Reverse();

            return new NPCWay
            {
                From = To,
                To = From,
                Path = reversedPath
            };
        }

#if UNITY_EDITOR
        private const float DRAW_YOFFSET = 0.5f;

        public void DrawGizmos()
        {
            if (From == null || To == null)
            {
                return;
            }

            float lineWidth = 10f;
            Vector3 prev = From.Position + Vector3.up * DRAW_YOFFSET;

            foreach (NPCRootPoint point in Path)
            {
                if (point != null)
                {
                    Vector3 current = point.Position + Vector3.up * DRAW_YOFFSET;
                    UnityEditor.Handles.DrawBezier(prev, current, prev, current, Color.blue, null, lineWidth);
                    prev = current;
                }
            }

            Vector3 finalPos = To.Position + Vector3.up * DRAW_YOFFSET;
            UnityEditor.Handles.DrawBezier(prev, finalPos, prev, finalPos, Color.blue, null, lineWidth);
        }
#endif
    }
}