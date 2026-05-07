using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems.Sound;
using BigProject.Utilities;
using System.Collections;
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

        private SoundsManager _soundsManager;
        private Coroutine _initCoroutine;
        private int _initMaxWait = 10;
        private int _initCounter = 0;

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
        }

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_deerPrefab);
            ExceptionUtilities.ThrowIfNullFormat(_deerConainer);

            if (_spawnPoints == null || _spawnPoints.Count == 0 || _deerCount == 0)
                return;

            _deerCount = Mathf.Min(_deerCount, _spawnPoints.Count);

            _initCoroutine = StartCoroutine(WaitForInitRoutine());
        }

        private IEnumerator WaitForInitRoutine()
        {
            while (_soundsManager == null)
            {
                _initCounter++;

                yield return new WaitForSeconds(1f);

                if (_initCounter >= _initMaxWait)
                {
                    ExceptionUtilities.ThrowIfNullFormat(_soundsManager);
                    break;
                }
            }

            SpawnDeers();

            _initCoroutine = null;
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
                NPCDeer deer = Instantiate(_deerPrefab, point.position, point.rotation, _deerConainer);

                EnvironmentSound sound = deer.GetComponentInChildren<EnvironmentSound>();
                ExceptionUtilities.ThrowIfNullFormat(sound);
                deer.GetComponentInChildren<EnvironmentSound>().Init(_soundsManager);
            }
        }

        private void OnDisable()
        {
            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
                _initCoroutine = null;
            }
        }
    }
}