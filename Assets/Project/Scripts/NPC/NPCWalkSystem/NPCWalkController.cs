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
        [SerializeField] private NPCRouteService _routeService;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Animator _animator;

        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _minDelayTimeInPoint = 1f;
        [SerializeField] private float _maxDelayTimeInPoint = 5f;

        private Queue<NPCRootPoint> _currentRoute;
        private Coroutine _walkCoroutine;

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
            if (_walkCoroutine != null)
            {
                StopCoroutine(_walkCoroutine);
                _walkCoroutine = null;
            }

            if (_agent != null && _agent.isActiveAndEnabled)
                _agent.ResetPath();
        }

        private void GoToNearestAttractionPoint()
        {
            if (_walkCoroutine != null)
                StopCoroutine(_walkCoroutine);

            _nextPoint = _routeService.GetNearstAttractionPoint(transform.position);

            if (_nextPoint == null)
                throw new NullReferenceException();

            if (Vector3.Distance(_nextPoint.Position, transform.position) > _agent.stoppingDistance)
                _walkCoroutine = StartCoroutine(GoTo());
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

            _walkCoroutine = StartCoroutine(FollowRoute());
        }

        private void PlayAnimation(NPCAnimation animation)
        {
            _animator.SetTrigger(animation.ToString());
        }

        private IEnumerator GoTo()
        {
            _agent.SetDestination(_nextPoint.Position);
            PlayAnimation(NPCAnimation.Walk1);

            yield return new WaitUntil(() =>
                !_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance);

            _lastAttractionPoint = _nextPoint as NPCAttractionPoint;

            yield return OnDestinationReached();
        }

        private IEnumerator FollowRoute()
        {
            while (_currentRoute.Count > 0)
            {
                _nextPoint = _currentRoute.Dequeue();

                _agent.SetDestination(_nextPoint.Position);
                PlayAnimation(NPCAnimation.Walk1);

                yield return new WaitUntil(() =>
                    !_agent.pathPending &&
                    _agent.remainingDistance <= _agent.stoppingDistance);

                if (_nextPoint is NPCAttractionPoint attractionPoint)
                    _lastAttractionPoint = attractionPoint;
            }

            yield return OnDestinationReached();
        }

        private IEnumerator OnDestinationReached()
        {
            PlayAnimation(_lastAttractionPoint.TargetAnimation);
            yield return new WaitForSeconds(Random.Range(_minDelayTimeInPoint, _maxDelayTimeInPoint) * _lastAttractionPoint.DelayModificator);

            GoToNextAttractionPoint();
        }
    }
}