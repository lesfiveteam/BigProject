using Assets.Project.Scripts.NPC.NPCWalkSystem;
using BigProject.Utilities;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Fish
{
    public class NPCFishManager : MonoBehaviour
    {
        [SerializeField] private NPCFishPool _fishPool;
        [SerializeField] private NPCRouteService _routeService;
        [SerializeField] private NPCAttractionPoint _startPoint;

        [SerializeField, Min(0)] private float _minTimeToSpawn = 7;
        [SerializeField, Min(0)] private float _maxTimeToSpawn = 15;

        private Coroutine _spawnCoroutine;

        private bool _isSpawn;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_fishPool);
            ExceptionUtilities.ThrowIfNullFormat(_routeService);
            ExceptionUtilities.ThrowIfNullFormat(_startPoint);

            Queue<NPCRoutePoint> currentRoute = _routeService.GetRandomRouteFrom(_startPoint);

            ExceptionUtilities.ThrowIfEmptyCollection(currentRoute, $"{name}.{nameof(currentRoute)}");

            List<NPCRoutePoint> routeList = new(currentRoute);

            Vector3[] positions = new Vector3[routeList.Count];
            for (int i = 0; i < routeList.Count; i++)
                positions[i] = routeList[i].Position;

            float[] distances = new float[positions.Length - 1];
            for (int i = 0; i < positions.Length - 1; i++)
                distances[i] = Vector3.Distance(positions[i], positions[i + 1]);

            _fishPool.Init(positions, distances);

            DOTween.SetTweensCapacity(1000, 100);

            _isSpawn = true;
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (_isSpawn)
            {
                yield return new WaitForSeconds(Random.Range(_minTimeToSpawn, _maxTimeToSpawn));

                NPCFish fish = _fishPool.Get();
                fish.transform.SetPositionAndRotation(_startPoint.Position, Quaternion.identity);
                fish.SwimToSea();
            }
        }

        private void OnDisable()
        {
            _isSpawn = false;

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }
    }
}