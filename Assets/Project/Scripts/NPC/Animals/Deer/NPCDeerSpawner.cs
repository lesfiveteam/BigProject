using BigProject.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Deer
{
    public class NPCDeerSpawner : MonoBehaviour
    {
        [SerializeField] private NPCDeer _deerPrefab;
        [SerializeField] private List<Transform> _spawnPoints;
        [SerializeField, Range(0, 5)] private int _deerCount;
        [SerializeField] private Transform _deerConainer;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_deerPrefab);
            ExceptionUtilities.ThrowIfNullFormat(_deerConainer);

            if (_spawnPoints == null || _spawnPoints.Count == 0 || _deerCount == 0)
                return;

            _deerCount = Mathf.Min(_deerCount, _spawnPoints.Count);

            SpawnDeers();
        }

        private void SpawnDeers()
        {
            List<Transform> availablePoints = new(_spawnPoints);

            for (int i = availablePoints.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (availablePoints[i], availablePoints[randomIndex]) = (availablePoints[randomIndex], availablePoints[i]);
            }

            for (int i = 0; i < _deerCount; i++)
            {
                Transform point = availablePoints[i];
                Instantiate(_deerPrefab, point.position, point.rotation, _deerConainer);
            }
        }
    }
}