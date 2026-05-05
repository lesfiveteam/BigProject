using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BigProject.Player
{
    public class WalkDustEffect : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [Tooltip("Все партикл системы")]
        [SerializeField] private ParticleSystem[] _dustParticleSystems;
        [Tooltip("Расстояние, через которое будет появляться эффект")]
        [SerializeField] private float _distanceInterval = 1f;
        [Tooltip("Сколько максимально эффектов может появиться за один раз")]
        [SerializeField] private int _maxParticlesPerInterval = 2;
        [Tooltip("Разброс эффектов вокруг точки спавна (чтобы не все в одном месте появлялись)")]
        [SerializeField] private Vector3 _effectOffset = new Vector3(0.15f, 0f, 0.15f);
        [Tooltip("Шанс, что каждый из выбранных скриптом эффектов появится")]
        [SerializeField][Range(0, 1f)] private float _effectSpawnChance = 1f;
        [Tooltip("Задержка между пылинками в одном эффекте")]
        [SerializeField] private float _nextEffectDelay;

        private Vector3 _previousPosition;
        private float _distanceAccumulator;
        private WaitForSeconds _distanceCheckWait = new(DISTANCE_CHECK_TIME);
        private WaitForSeconds _nextEffectWait;
        private Coroutine _nextEffectRoutine;

        private const float DISTANCE_CHECK_TIME = 0.05f;
        private const float MAX_DISTANCE_EFFECT = 3f;

        private void Awake()
        {
            _previousPosition = transform.position;
            _nextEffectWait = new(_nextEffectDelay);
        }

        private void Start()
        {
            StartCoroutine(DistanceCheckRoutine());
        }

        private IEnumerator DistanceCheckRoutine()
        {
            while (true)
            {
                if (_nextEffectRoutine != null)
                {
                    StopCoroutine(_nextEffectRoutine);
                }

                Vector3 currentPos = transform.position;
                _distanceAccumulator += Vector3.Distance(currentPos, _previousPosition);
                _previousPosition = currentPos;

                if (_distanceAccumulator >= _distanceInterval)
                {
                    if (MAX_DISTANCE_EFFECT > _distanceAccumulator)//In case we've teleported
                    {
                        _nextEffectRoutine = StartCoroutine(EmitDusts());
                    }

                    _distanceAccumulator = 0;
                }

                yield return _distanceCheckWait;
            }
        }

        private IEnumerator EmitDusts()
        {
            int totalSystems = _dustParticleSystems.Length;
            int particlesToEmit = Mathf.Min(_maxParticlesPerInterval, totalSystems);
            List<int> selectedIndices = GetRandomDistinctIndices(totalSystems, particlesToEmit);

            foreach (int index in selectedIndices)
            {
                if (Random.Range(0f, 1f) > _effectSpawnChance)
                {
                    continue;
                }

                ParticleSystem ps = _dustParticleSystems[index];
                Vector3 originalPos = transform.position;
                Vector3 offset = new Vector3(Random.Range(-_effectOffset.x, _effectOffset.x),
                    0f, Random.Range(-_effectOffset.z, _effectOffset.z));
                ps.transform.position = originalPos + offset;
                ps.Emit(1);
                yield return _nextEffectWait;
            }
        }

        private List<int> GetRandomDistinctIndices(int totalCount, int countToSelect)
        {
            List<int> allIndices = new List<int>(totalCount);

            for (int i = 0; i < totalCount; i++)
            {
                allIndices.Add(i);
            }

            for (int i = 0; i < countToSelect; i++)
            {
                int randomIndex = Random.Range(i, totalCount);
                (allIndices[randomIndex], allIndices[i]) = (allIndices[i], allIndices[randomIndex]);
            }

            return allIndices.GetRange(0, countToSelect);
        }
    }
}