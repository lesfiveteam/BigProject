using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Systems.Sound;
using BigProject.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Project.Scripts.NPC.Animals.Chicken
{
    public class NPCChickenSpawner : MonoBehaviour
    {
        private const int INIT_MAX_WAIT = 10;
        private const int SPAWN_COUNT_TRY = 30;
        private const float CHICKEN_SPAWN_DISTANCE = 3f;

        [SerializeField] private NPCCock _cockPrefab;
        [SerializeField] private NPCChicken _chickenPrefab;
        [SerializeField] private Transform _chickenContainer;
        [SerializeField] private List<NPCPeckPoint> _peckPoints;

        private SoundsManager _soundsManager;
        private List<NPCCock> _cocks = new();
        private Coroutine _initCoroutine;
        private WaitForSeconds _initWaitTime = new(1f);

        private int _initCounter = 0;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_cockPrefab);
            ExceptionUtilities.ThrowIfNullFormat(_chickenPrefab);
            ExceptionUtilities.ThrowIfNullFormat(_chickenContainer);
            ExceptionUtilities.ThrowIfEmptyCollection(_peckPoints, "_peckPoints");

            _initCoroutine = StartCoroutine(WaitForInitRoutine());
        }

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
        }

        private IEnumerator WaitForInitRoutine()
        {
            while (_soundsManager == null)
            {
                _initCounter++;

                yield return _initWaitTime;

                if (_initCounter >= INIT_MAX_WAIT)
                {
                    ExceptionUtilities.ThrowIfNullFormat(_soundsManager);
                    break;
                }
            }

            SpawnProcess();

            _initCoroutine = null;
        }

        private void SpawnProcess()
        {
            if (_peckPoints.Count < 1)
                return;

            foreach (NPCPeckPoint peckPoint in _peckPoints)
            {
                if (peckPoint != null && peckPoint.IsSpawnPoin)
                {
                    SpawnCock(peckPoint);
                    peckPoint.IsOccupied = true;
                }
            }
        }

        private void SpawnCock(NPCPeckPoint spawnPoint)
        {
            NPCCock cock = Instantiate(_cockPrefab, spawnPoint.transform.position, Quaternion.identity, _chickenContainer);
            cock.Init(spawnPoint, this);
            _cocks.Add(cock);

            EnvironmentSound sound = cock.GetComponentInChildren<EnvironmentSound>();
            ExceptionUtilities.ThrowIfNullFormat(sound);
            sound.Init(_soundsManager);
            sound.PlaySound();

            for (int i = 0; i < spawnPoint.ChickenCount; i++)
                if (!SpawnChicken(cock, spawnPoint))
                    GameLogManager.Error(LogStr.ERROR_CANT_SPAWN_CHICKEN);
        }

        private bool SpawnChicken(NPCCock cock, NPCPeckPoint spawnPoint)
        {
            for (int i = 0; i < SPAWN_COUNT_TRY; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * CHICKEN_SPAWN_DISTANCE;
                Vector3 pointToCheck = cock.transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);

                int walkableMask = 1 << 0;

                if (NavMesh.SamplePosition(pointToCheck, out NavMeshHit hit, CHICKEN_SPAWN_DISTANCE, walkableMask))
                {
                    NPCChicken chicken = Instantiate(_chickenPrefab, hit.position, Quaternion.identity, _chickenContainer);
                    chicken.Init(cock, spawnPoint);

                    return true;
                }
            }

            return false;
        }

        public NPCPeckPoint GetNewPeckPoint(NPCPeckPoint currentPeckPoint)
        {
            NPCPeckPoint newPeckPoint = null;
            List<NPCPeckPoint> freePeckPoints = new(_peckPoints.Where(peckPoint => peckPoint.IsOccupied == false));

            if (freePeckPoints.Count == 0)
                return newPeckPoint;

            if (freePeckPoints.Count == 1)
                newPeckPoint = freePeckPoints[0];
            else if (freePeckPoints.Count > 1)
                newPeckPoint = freePeckPoints[Random.Range(0, freePeckPoints.Count)];

            currentPeckPoint.IsOccupied = false;
            newPeckPoint.IsOccupied = true;

            return newPeckPoint;
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