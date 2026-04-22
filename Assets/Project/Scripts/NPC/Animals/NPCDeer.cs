using Assets.Project.Scripts.Utilities;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Project.Scripts.NPC.Animals
{
    public class NPCDeer : MonoBehaviour, IScared
    {
        private readonly int RunTrigger = Animator.StringToHash("Run");
        private const string SPEED_MULTIPLIER = "speedMultiplier";
        private const string RUN_CLIP_NAME = "run1";
        private const int JUMP_FRAMES = 16;

        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;

        private Coroutine _runCoroutine;
        private AnimationClip _clipRun;

        private int _jumpsCount = 6;
        private float _jumpDistance = 10f;

        private int _attemptsForSearch = 2;
        private float _angleStep = 2f;
        private float _searchRadius = 1f;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);
            ExceptionUtilities.ThrowIfNullFormat(_agent);

            _clipRun = _animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == RUN_CLIP_NAME);

            _agent.updateRotation = false;
        }

        public void Scare(Vector3 dangerPosition)
        {
            if (_runCoroutine != null)
                return;

            Vector3 directionFromDanger = (transform.position - dangerPosition).normalized;

            _runCoroutine = StartCoroutine(ScaredRun(directionFromDanger));
        }

        private IEnumerator ScaredRun(Vector3 direction)
        {
            NavMeshPath path = new();

            while (_jumpsCount > 0)
            {
                _jumpsCount--;

                bool pointFound = false;

                yield return StartCoroutine(TryFindJumpPointCoroutine(direction, path, (isFound) => pointFound = isFound));

                direction = (path.corners.Last() - transform.position).normalized;

                if (pointFound)
                {
                    float jumpTime = NavMeshUtils.GetPathLength(path) / _agent.speed;
                    float frameDuration = _clipRun.length / _clipRun.frameRate;
                    float timeForJumpFrames = frameDuration * JUMP_FRAMES;
                    float timeForEndJump = _clipRun.length - timeForJumpFrames;
                    float animationSpeedMultiplier = timeForJumpFrames / jumpTime;
                    _animator.SetFloat(SPEED_MULTIPLIER, animationSpeedMultiplier);
                    _animator.SetTrigger(RunTrigger);

                    Vector3 pointToRun = path.corners.Last();
                    transform.rotation = Quaternion.LookRotation(pointToRun - transform.position);
                    _agent.SetDestination(pointToRun);

                    yield return new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);

                    yield return new WaitForSeconds(timeForEndJump);
                }
                else
                {
                    Debug.LogWarningFormat(LogStr.WARNING_CANT_FIND_PATH, gameObject.name, transform.position);
                    break;
                }
            }

            Destroy(gameObject);
        }

        private IEnumerator TryFindJumpPointCoroutine(Vector3 direction, NavMeshPath path, Action<bool> callback)
        {
            Vector3 origin = transform.position;
            int attemptsCount = 0;

            for (float angle = 0; angle <= 180; angle += _angleStep)
            {
                if (TryGetPointWithDirection(direction, angle, origin, path))
                {
                    callback?.Invoke(true);
                    yield break;
                }

                attemptsCount++;

                if (TryGetPointWithDirection(direction, -angle, origin, path))
                {
                    callback?.Invoke(true);
                    yield break;
                }

                attemptsCount++;

                if (attemptsCount >= 10)
                {
                    attemptsCount = 0;
                    yield return new WaitWhile(() => Time.timeScale == 0);
                }
            }

            callback?.Invoke(false);
        }

        private bool TryGetPointWithDirection(Vector3 baseDirection, float angle, Vector3 origin, NavMeshPath path)
        {
            Vector3 rotatedDir = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
            Vector3 targetPoint = origin + rotatedDir * _jumpDistance;

            if (NavMeshUtils.TryGetRandomPointAtDistance(origin, targetPoint, _searchRadius, path, _attemptsForSearch) && path.corners.Length == 2)
            {
                return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            if (_runCoroutine != null)
                StopCoroutine(_runCoroutine);
        }
    }
}