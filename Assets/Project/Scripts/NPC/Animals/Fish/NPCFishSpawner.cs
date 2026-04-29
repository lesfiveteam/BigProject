using Assets.Project.Scripts.NPC.NPCWalkSystem;
using BigProject.Utilities;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Fish
{
    public class NPCFishSpawner : MonoBehaviour
    {
        [SerializeField] private NPCFish _fishPrefab;
        [SerializeField] private NPCRouteService _routeService;
        [SerializeField] private NPCAttractionPoint _startPoint;
        [SerializeField] private Transform _parent;

        [SerializeField, Min(0)] private float _minTimeToSpawn;
        [SerializeField, Min(0)] private float _maxTimeToSpawn;

        private Coroutine _spawnCoroutine;
        private Queue<NPCRootPoint> _currentRoute;

        private bool _isSpawn;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_fishPrefab);
            ExceptionUtilities.ThrowIfNullFormat(_routeService);
            ExceptionUtilities.ThrowIfNullFormat(_startPoint);

            DOTween.SetTweensCapacity(1000, 100);

            _currentRoute = _routeService.GetRandomRouteFrom(_startPoint.Id);

            if (_currentRoute == null || _currentRoute.Count == 0)
            {
                ExceptionUtilities.ThrowIfEmptyCollection(_currentRoute, "_currentRoute");
                _isSpawn = false;
                return;
            }

            _isSpawn = true;
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        public void StopSpawning()
        {
            _isSpawn = false;

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            while (_isSpawn)
            {
                yield return new WaitForSeconds(Random.Range(_minTimeToSpawn, _maxTimeToSpawn));

                Queue<NPCRootPoint> routeCopy = new Queue<NPCRootPoint>(_currentRoute);

                NPCFish fish = Instantiate(_fishPrefab, _startPoint.Position, Quaternion.identity, _parent);
                fish.Init(routeCopy);
            }
        }

        private void OnDestroy()
        {
            StopSpawning();
        }
    }
}