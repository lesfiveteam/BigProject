using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.Utilities
{
    public class NavMeshUtils
    {
        private static WaitForEndOfFrame _cachedEndOfFrame = new WaitForEndOfFrame();
        private static readonly int YIELD_ATTEMPTS_INTERVAL = 10;

        public static float GetPathLength(NavMeshPath path)
        {
            float length = 0;

            if (path.corners.Length > 1)
                for (int i = 1; i < path.corners.Length; i++)
                    length += Vector3.Distance(path.corners[i - 1], path.corners[i]);

            return length;
        }

        public static bool TryGetPath(NavMeshAgent agent, Vector3 targetPosition, NavMeshPath pathToTarget)
        {
            if (agent.CalculatePath(targetPosition, pathToTarget)
                && pathToTarget.status != NavMeshPathStatus.PathInvalid)
                return true;

            return false;
        }

        public static IEnumerator TryGetRandomPointAtDistanceRoutine(
            Vector3 currentPosition,
            Vector3 center,
            float radius,
            NavMeshPath path,
            Action<bool> callback = null,
            int maxAttempts = 30)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * radius;
                Vector3 pointToCheck = center + new Vector3(randomOffset.x, 0, randomOffset.y);

                if (NavMesh.SamplePosition(pointToCheck, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    path.ClearCorners();

                    if (NavMesh.CalculatePath(currentPosition, hit.position, NavMesh.AllAreas, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete && path.corners.Length >= 2)
                        {
                            callback?.Invoke(true);
                            yield break;
                        }
                    }
                }

                if ((i + 1) % YIELD_ATTEMPTS_INTERVAL == 0)
                    yield return _cachedEndOfFrame;
            }

            callback?.Invoke(false);
        }

        public static IEnumerator TryGetRandomPointInCircleRoutine(
            Vector3 currentPosition,
            Vector3 targetPosition,
            float searchRadius,
            Vector3 restrictPoint,
            float restrictionRadius,
            NavMeshPath path,
            Action<bool> callback = null,
            int maxAttempts = 30)
        {
            float maxDistance = 50f;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * searchRadius;
                Vector3 pointToCheck = targetPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

                if (NavMesh.SamplePosition(pointToCheck, out NavMeshHit hit, maxDistance, NavMesh.AllAreas)
                    && pointToCheck.IsInCircle(restrictPoint, restrictionRadius))
                {
                    path.ClearCorners();

                    if (NavMesh.CalculatePath(currentPosition, hit.position, NavMesh.AllAreas, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            callback?.Invoke(true);
                            yield break;
                        }
                    }
                }

                if ((i + 1) % YIELD_ATTEMPTS_INTERVAL == 0)
                    yield return _cachedEndOfFrame;
            }

            callback?.Invoke(false);
        }

        public static Vector3 AddRandomDeviation(Vector3 direction, float maxAngleDegrees)
        {
            float angleDegrees = Random.Range(-maxAngleDegrees, maxAngleDegrees);

            Quaternion rotation = Quaternion.AngleAxis(angleDegrees, Vector3.up);
            Vector3 newDirection = rotation * direction.normalized;

            return newDirection.normalized;
        }

        public static void CreateDebugSphere(Vector3 position) => CreateDebugSphere(position, Color.blue);

        public static void CreateDebugSphere(Vector3 position, Color color)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();

            sphere.transform.position = position;
            renderer.material.color = color;
        }
    }
}
