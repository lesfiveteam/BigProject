using BigProject.NPC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.NPC.NPCWalkSystem
{
    public class NPCWalkController : MonoBehaviour
    {
        private const float MAX_DISTANCE_TO_NAV_MESH = 3f;

        protected readonly int WalkBool = Animator.StringToHash("isWalk");

        [SerializeField] private NPCRouteService _routeService;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Animator _animator;
        [SerializeField] private NPCController _NPCController;

        [SerializeField] private NPCAnimationMovement _movementAnimation;

        [SerializeField] private float _minDelayTimeInPoint = 1f;
        [SerializeField] private float _maxDelayTimeInPoint = 5f;

        private Queue<NPCRootPoint> _currentRoute;
        private NPCAttractionPoint _lastAttractionPoint;
        private NPCAttractionPoint _nextAttractionPoint;

        private Coroutine _walkCoroutine;
        private Coroutine _lookCoroutine;

        private WaitWhile _cachedWaitWhilePause = new(() => Time.timeScale == 0);
        private WaitUntil _cachedArrivedCondition;

        private bool _isInited;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            if (_isInited) 
                return;

            _cachedArrivedCondition = new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);
            _isInited = true;
        }

        public void StartWalk()
        {
            Init();

            if (_nextAttractionPoint == null || _currentRoute == null)
            {
                GoToNearestAttractionPoint();
            }
            else
            {
                GoToNextAttractionPoint();
            }
        }

        public void StopWalk()
        {
            _animator.SetBool(WalkBool, false);

            ClearCoroutine(ref _walkCoroutine);
            ClearCoroutine(ref _lookCoroutine);

            if (_agent != null && _agent.isActiveAndEnabled)
                _agent.ResetPath();
        }

        public void GoTo(Vector3 target, Action onComplete)
        {
            ClearCoroutine(ref _walkCoroutine);

            _walkCoroutine = StartCoroutine(GoToRoutine(target, onComplete));
        }

        private IEnumerator GoToRoutine(Vector3 target, Action onComplete)
        {
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, MAX_DISTANCE_TO_NAV_MESH, NavMesh.AllAreas))
            {
                target = hit.position;
            }

            _agent.SetDestination(target);
            _animator.SetBool(WalkBool, true);

            yield return _cachedArrivedCondition;

            _animator.SetBool(WalkBool, false);

            onComplete?.Invoke();
        }

        public void LookAt(Vector3 targetPoint, Action onComplete)
        {
            ClearCoroutine(ref _lookCoroutine);

            _lookCoroutine = StartCoroutine(LookAtRoutine(targetPoint, onComplete));
        }

        private IEnumerator LookAtRoutine(Vector3 targetPoint, Action onComplete)
        {
            Vector3 targetDirection = targetPoint - _agent.transform.position;
            targetDirection.y = 0f;

            if (targetDirection == Vector3.zero)
            {
                onComplete?.Invoke();
                yield break;
            }

            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            float step = Time.deltaTime * _agent.angularSpeed;

            while (true)
            {
                float currentAngle = _agent.transform.eulerAngles.y;
                float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

                if (Mathf.Abs(angleDiff) <= 0.01f)
                    break;

                float newAngle = currentAngle + Mathf.Sign(angleDiff) * Mathf.Min(Mathf.Abs(angleDiff), step);
                _agent.transform.rotation = Quaternion.Euler(0f, newAngle, 0f);

                yield return _cachedWaitWhilePause;
            }

            onComplete?.Invoke();
        }

        private void GoToNearestAttractionPoint()
        {
            ClearCoroutine(ref _walkCoroutine);

            _nextAttractionPoint = _routeService.GetNearstAttractionPoint(transform.position);

            if (_nextAttractionPoint == null)
                throw new NullReferenceException();

            if ((_nextAttractionPoint.Position - transform.position).sqrMagnitude > _agent.stoppingDistance * _agent.stoppingDistance)
            {
                _walkCoroutine = StartCoroutine(GoToNearestAttractionPointRoutine());
            }
            else
            {
                _lastAttractionPoint = _nextAttractionPoint;
                _walkCoroutine = StartCoroutine(OnAttractionPointReachedRoutine());
            }
        }

        private IEnumerator GoToNearestAttractionPointRoutine()
        {
            _agent.SetDestination(_nextAttractionPoint.Position);
            _animator.SetBool(WalkBool, true);

            yield return _cachedArrivedCondition;

            _lastAttractionPoint = _nextAttractionPoint;

            yield return OnAttractionPointReachedRoutine();
        }

        private async void GoToNextAttractionPoint()
        {
            await _NPCController.AgentOn();

            ClearCoroutine(ref _walkCoroutine);

            _walkCoroutine = StartCoroutine(GoToNextAttractionPointRoutine());
        }

        private IEnumerator GoToNextAttractionPointRoutine()
        {
            NPCRootPoint nextPoint = null;

            while (_currentRoute.Count > 0)
            {
                nextPoint = _currentRoute.Dequeue();

                _agent.SetDestination(nextPoint.Position);
                _animator.SetBool(WalkBool, true);

                yield return _cachedArrivedCondition;
            }

            if (nextPoint is not null and NPCAttractionPoint attractionPoint)
                _lastAttractionPoint = attractionPoint;

            yield return OnAttractionPointReachedRoutine();
        }

        private IEnumerator OnAttractionPointReachedRoutine()
        {
            _animator.SetBool(WalkBool, false);

            if (_lastAttractionPoint != null)
                _animator.SetTrigger(_lastAttractionPoint.TargetAnimation.ToString());

            _NPCController.ObstacleOn();

            yield return new WaitForSeconds(Random.Range(_minDelayTimeInPoint, _maxDelayTimeInPoint) * _lastAttractionPoint.DelayModificator);

            _currentRoute = _routeService.GetRandomRouteFrom(_lastAttractionPoint.Id);

            GoToNextAttractionPoint();
        }

        private void ClearCoroutine(ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        private void OnDisable()
        {
            StopWalk();
        }
    }
}