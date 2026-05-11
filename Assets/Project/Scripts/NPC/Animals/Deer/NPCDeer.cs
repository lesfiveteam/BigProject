using Assets.Project.Scripts.Utilities;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Project.Scripts.NPC.Animals.Deer
{
    public class NPCDeer : MonoBehaviour, IScared
    {
        private const int JUMP_FRAMES = 16;
        private const float JUMP_DISTANCE = 10f;

        private const int ATTEMPT_FOR_SEARCH = 2;
        private const float ANGLE_STEP = 2f;
        private const float SEARCH_RADIUS = 1f;

        private const string RUN_CLIP_NAME = "run1";
        private readonly int _speedFloat = Animator.StringToHash("speedMultiplier");
        private readonly int _runTrigger = Animator.StringToHash("Run");

        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;

        private int _jumpCount = 6;

        private Coroutine _runCoroutine;

        private WaitWhile _cachedWaitWhilePause = new(() => Time.timeScale == 0);
        private WaitUntil _cachedArrivedCondition;
        private WaitForSeconds _waitForEndJump;

        private float _timeForJumpFrames;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);
            ExceptionUtilities.ThrowIfNullFormat(_agent);

            _cachedArrivedCondition = new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);
            AnimationClip clipRun = _animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == RUN_CLIP_NAME);
            float frameDuration = clipRun.length / clipRun.frameRate;
            _timeForJumpFrames = frameDuration * JUMP_FRAMES;
            float timeForEndJump = clipRun.length - _timeForJumpFrames;
            _waitForEndJump = new(timeForEndJump);

            _agent.updateRotation = false;
        }

        public void Scare(Transform danger)
        {
            if (_runCoroutine != null)
                return;

            _runCoroutine = StartCoroutine(ScaredRunRoutine(danger));
        }

        private IEnumerator ScaredRunRoutine(Transform danger)
        {
            NavMeshPath path = new();
            Vector3 directionFromDanger;
            Vector3 pointToRun;

            while (_jumpCount > 0)
            {
                _jumpCount--;

                bool pointFound = false;

                directionFromDanger = (transform.position - danger.position).normalized;

                yield return TryFindJumpPointRoutine(
                    directionFromDanger,
                    path,
                    (isFound) => pointFound = isFound);

                if (pointFound)
                {
                    float jumpTime = NavMeshUtils.GetPathLength(path) / _agent.speed;
                    float animationSpeedMultiplier = _timeForJumpFrames / jumpTime;
                    _animator.SetFloat(_speedFloat, animationSpeedMultiplier);
                    _animator.SetTrigger(_runTrigger);

                    pointToRun = path.corners.Last();
                    transform.rotation = Quaternion.LookRotation(pointToRun - transform.position);
                    _agent.SetDestination(pointToRun);

                    yield return _cachedArrivedCondition;

                    yield return _waitForEndJump;
                }
                else
                {
                    Debug.LogWarningFormat(LogStr.WARNING_CANT_FIND_PATH, gameObject.name, transform.position);
                    break;
                }
            }

            Destroy(gameObject);
        }

        private IEnumerator TryFindJumpPointRoutine(Vector3 direction, NavMeshPath path, Action<bool> callback)
        {
            Vector3 origin = transform.position;
            int attemptsCount = 0;
            Vector3 rotatedDir;
            Vector3 targetPoint;
            bool isFounded = false;

            for (float angle = 0; angle <= 180; angle += ANGLE_STEP)
            {
                rotatedDir = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                targetPoint = origin + (rotatedDir * JUMP_DISTANCE);

                yield return NavMeshUtils.TryGetRandomPointAtDistanceRoutine(
                    origin,
                    targetPoint,
                    SEARCH_RADIUS,
                    path,
                    (founded) => isFounded = founded,
                    ATTEMPT_FOR_SEARCH);

                if (isFounded && path.corners.Length == 2)
                {
                    callback?.Invoke(true);
                    yield break;
                }

                attemptsCount++;

                rotatedDir = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                targetPoint = origin + (rotatedDir * JUMP_DISTANCE);

                yield return NavMeshUtils.TryGetRandomPointAtDistanceRoutine(
                    origin,
                    targetPoint,
                    SEARCH_RADIUS,
                    path,
                    (founded) => isFounded = founded,
                    ATTEMPT_FOR_SEARCH);

                if (isFounded && path.corners.Length == 2)
                {
                    callback?.Invoke(true);
                    yield break;
                }

                attemptsCount++;

                if (attemptsCount >= 10)
                {
                    attemptsCount = 0;
                    yield return _cachedWaitWhilePause;
                }
            }

            callback?.Invoke(false);
        }

        private void OnDisable()
        {
            if (_runCoroutine != null)
            {
                StopCoroutine(_runCoroutine);
                _runCoroutine = null;
            }
        }
    }
}