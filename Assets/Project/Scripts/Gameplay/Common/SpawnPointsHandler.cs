using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BigProject.Gameplay.Common
{
    public class SpawnPointsHandler : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> _points;

        public bool TryGetSpawnTransform(int pointId, out Transform spawnTransform)
        {
            spawnTransform = _points.ElementAtOrDefault(pointId);
            return spawnTransform != null;
        }

        public int GetNearestPointId(Vector3 position)
        {
            int closestId = 0;
            float minSqrDistance = float.MaxValue;

            for (int i = 0; i < _points.Count; i++)
            {
                Vector3 direction = _points[i].position - position;
                float sqrDistance = direction.sqrMagnitude;

                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    closestId = i;
                }
            }

            return closestId;
        }
    }
}
