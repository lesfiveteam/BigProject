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
        private const float EPSILON = 1f;

        [SerializeField] private NPCRouteService _routeService;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Animator _animator;

        [SerializeField] private NPCAnimationMovement _movementAnimation;
        protected readonly int WalkBool = Animator.StringToHash("isWalk");

        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _minDelayTimeInPoint = 1f;
        [SerializeField] private float _maxDelayTimeInPoint = 5f;

        private Queue<NPCRootPoint> _currentRoute;
        private Coroutine _walkCoroutine;
        private Coroutine _lookCoroutine;

        private NPCAttractionPoint _lastAttractionPoint;
        private NPCRootPoint _nextPoint;

        private void Awake()
        {
            _agent.speed = _moveSpeed;
        }

        public void StartWalk()
        {
            if (_currentRoute != null && _nextPoint != null)
            {
                StartWalkCoroutine();
            }
            else
            {
                GoToNearestAttractionPoint();
            }
        }

        public void StopWalk()
        {
            _animator.SetBool(WalkBool, false);

            if (_walkCoroutine != null)
            {
                StopCoroutine(_walkCoroutine);
                _walkCoroutine = null;
            }

            if (_lookCoroutine != null)
            {
                StopCoroutine(_lookCoroutine);
                _lookCoroutine = null;
            }

            if (_agent != null && _agent.isActiveAndEnabled)
                _agent.ResetPath();
        }

        public void GoTo(Vector3 target, Action onComplete)
        {
            StopWalk();

            _walkCoroutine = StartCoroutine(GoToTalkRoutine(target, onComplete));
        }

        public void LookAt(Vector3 target, Action onComplete)
        {
            _lookCoroutine = StartCoroutine(LookAtRoutine(target, onComplete));
        }

        private IEnumerator GoToTalkRoutine(Vector3 target, Action onComplete)
        {
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, MAX_DISTANCE_TO_NAV_MESH, NavMesh.AllAreas))
            {
                target = hit.position;
            }

            _agent.SetDestination(target);
            _animator.SetBool(WalkBool, true);

            yield return new WaitUntil(() =>
                !_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance);

            _animator.SetBool(WalkBool, false);

            onComplete?.Invoke();
        }

        private IEnumerator LookAtRoutine(Vector3 target, Action onComplete)
        {
            Vector3 lookDir = target - _agent.transform.position;
            lookDir.y = 0f;

            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);

                while (Quaternion.Angle(_agent.transform.rotation, targetRot) > EPSILON)
                {
                    _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRot, Time.deltaTime * _agent.angularSpeed);

                    yield return new WaitWhile(() => Time.timeScale == 0);
                }
            }

            onComplete?.Invoke();
        }

        private void GoToNearestAttractionPoint()
        {
            if (_walkCoroutine != null)
                StopCoroutine(_walkCoroutine);

            _nextPoint = _routeService.GetNearstAttractionPoint(transform.position);

            if (_nextPoint == null)
                throw new NullReferenceException();

            if (Vector3.Distance(_nextPoint.Position, transform.position) > _agent.stoppingDistance)
                _walkCoroutine = StartCoroutine(GoToRoutine());
        }

        private void GoToNextAttractionPoint()
        {
            _currentRoute = _routeService.GetRandomRouteFrom(_lastAttractionPoint.Id);

            StartWalkCoroutine();
        }

        private void StartWalkCoroutine()
        {
            if (_walkCoroutine != null)
                StopCoroutine(_walkCoroutine);

            _walkCoroutine = StartCoroutine(FollowRouteRoutine());
        }

        private IEnumerator GoToRoutine()
        {
            _agent.SetDestination(_nextPoint.Position);
            _animator.SetBool(WalkBool, true);

            yield return new WaitUntil(() =>
                !_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance);

            _lastAttractionPoint = _nextPoint as NPCAttractionPoint;

            yield return OnDestinationReachedRoutine();
        }

        private IEnumerator FollowRouteRoutine()
        {
            while (_currentRoute.Count > 0)
            {
                _nextPoint = _currentRoute.Dequeue();

                _agent.SetDestination(_nextPoint.Position);
                _animator.SetBool(WalkBool, true);

                yield return new WaitUntil(() =>
                    !_agent.pathPending &&
                    _agent.remainingDistance <= _agent.stoppingDistance);

                if (_nextPoint is NPCAttractionPoint attractionPoint)
                    _lastAttractionPoint = attractionPoint;
            }

            yield return OnDestinationReachedRoutine();
        }

        private IEnumerator OnDestinationReachedRoutine()
        {
            _animator.SetBool(WalkBool, false);
            _animator.SetTrigger(_lastAttractionPoint.TargetAnimation.ToString());
            yield return new WaitForSeconds(Random.Range(_minDelayTimeInPoint, _maxDelayTimeInPoint) * _lastAttractionPoint.DelayModificator);

            GoToNextAttractionPoint();
        }
    }
}