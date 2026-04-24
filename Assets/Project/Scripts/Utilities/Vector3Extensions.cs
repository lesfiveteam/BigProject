using UnityEngine;

namespace Assets.Project.Scripts.Utilities
{
    public static class Vector3Extensions
    {
        public static bool IsInCircle(this Vector3 point, Vector3 center, float radius)
        {
            Vector2 point2D = new Vector2(point.x, point.z);
            Vector2 center2D = new Vector2(center.x, center.z);

            float sqrRadius = radius * radius;
            float sqrDistance = (point2D - center2D).sqrMagnitude;

            return sqrDistance <= sqrRadius;
        }
    }
}
