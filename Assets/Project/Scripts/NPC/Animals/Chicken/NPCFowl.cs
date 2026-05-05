using Assets.Project.Scripts.Utilities;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.NPC.Animals.Chicken
{
    public class NPCFowl : MonoBehaviour, IScared, IUnscared
    {
        protected const float MIN_DELAY_CARVE_SWITCH_OFF = 0.2f;
        protected const string SPEED = "Speed";
        protected readonly int WalkBool = Animator.StringToHash("isWalk");

        [SerializeField] protected Animator _animator;
        [SerializeField] protected NavMeshAgent _agent;
        [SerializeField] protected NavMeshObstacle _obstacle;

        protected NPCPeckPoint _currentPeckPoint;
        protected NPCPeckPoint _newPeckPoint = null;
        protected Vector3 _pointToMove;

        protected float _peckPointRadius = 5f;
        protected float _peckRadius = 4f;
        protected float _minPeckTime = 2f;
        protected float _maxPeckTime = 5f;
        protected float _angleStep = 2f;
        protected float _escapeDistance = 3f;
        protected float _searchRadius = 1f;
        protected int _attemptsForSearch = 10;

        protected Coroutine _peckCoroutine = null;
        protected Coroutine _goToNewPeckPointCoroutine = null;
        protected Coroutine _scareCoroutine = null;
        protected Coroutine _returnToPeckPointCoroutine = null;

        protected bool _isAlive = false;
        protected bool _isScared = false;

        protected enum MoveSpeed
        {
            Walk,
            Run,
            Rush,
        }

        protected virtual void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_agent);
            ExceptionUtilities.ThrowIfNullFormat(_animator);
            ExceptionUtilities.ThrowIfNullFormat(_obstacle);
        }

        protected void SetMoveSpeed(MoveSpeed speed)
        {
            float modifier = speed switch
            {
                MoveSpeed.Walk => 1f,
                MoveSpeed.Run => 3f,
                MoveSpeed.Rush => 5f,
                _ => 1f
            };

            _animator.SetFloat(SPEED, modifier);
            _agent.acceleration = modifier;
        }

        protected IEnumerator PeckRoutine()
        {
            SetMoveSpeed(MoveSpeed.Walk);

            NavMeshPath path = new();

            float currentPeckRadius = _peckRadius;

            //PeckTimer(); // ToDo: need to check logic and fix

            while (_isAlive)
            {
                if (_isScared)
                {
                    yield return new WaitWhile(() => Time.timeScale == 0);
                    continue;
                }

                yield return new WaitForSeconds(Random.Range(_minPeckTime, _maxPeckTime));
                _obstacle.enabled = false;
                yield return new WaitForSeconds(MIN_DELAY_CARVE_SWITCH_OFF);
                _agent.enabled = true;
                yield return new WaitForEndOfFrame();

                if (NavMeshUtils.TryGetRandomPointInCircle(
                    transform.position,
                    transform.position,
                    currentPeckRadius, 
                    _currentPeckPoint.transform.position, 
                    _peckPointRadius, 
                    path))
                {
                    currentPeckRadius = _peckRadius;
                    Vector3 newPeckTarget = path.corners.Last();

                    _agent.SetDestination(newPeckTarget);

                    _animator.SetBool(WalkBool, true);

                    yield return new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);

                    _animator.SetBool(WalkBool, false);

                    _agent.enabled = false;
                    yield return new WaitForEndOfFrame();
                    _obstacle.enabled = true;
                }
                else
                {
                    currentPeckRadius++;
                }
            }
        }

        protected virtual void PeckTimer() { }

        protected IEnumerator GoToNewPeckPointRoutine(float delayTime = 0, bool beScared = false)
        {
            if (delayTime != 0)
                yield return new WaitForSeconds(delayTime);

            if (_peckCoroutine != null)
            {
                StopCoroutine(_peckCoroutine);
                _peckCoroutine = null;
            }

            if (_obstacle.enabled)
            {
                _obstacle.enabled = false;
                yield return new WaitForSeconds(MIN_DELAY_CARVE_SWITCH_OFF);
            }

            if (!_agent.enabled)
                _agent.enabled = true;

            yield return new WaitForEndOfFrame();

            SetMoveSpeed(MoveSpeed.Run);

            NavMeshPath path = new();

            if (beScared)
            {
                _pointToMove = _newPeckPoint.transform.position;

                float searchRadius = _peckPointRadius;

                NavMeshUtils.TryGetRandomPointInCircle(
                    transform.position,
                    _pointToMove,
                    _peckPointRadius,
                    _newPeckPoint.transform.position,
                    _peckPointRadius,
                    path);
            }
            else
            {
                Vector3 currentOffset = transform.position - _currentPeckPoint.transform.position;
                _pointToMove = _newPeckPoint.transform.position + currentOffset;

                float searchRadius = _peckPointRadius - currentOffset.magnitude;

                while (!NavMeshUtils.TryGetRandomPointInCircle(
                    transform.position, 
                    _pointToMove,
                    searchRadius,
                    _newPeckPoint.transform.position,
                    _peckPointRadius,
                    path))
                {
                    searchRadius++;
                }
            }

            if (path == null || path.corners.Length <= 1)
                Debug.LogError("Can't find point to new peck");

            _agent.SetDestination(path.corners.Last());
            _animator.SetBool(WalkBool, true);

            yield return new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);

            _animator.SetBool(WalkBool, false);

            _pointToMove = Vector3.zero;
            _currentPeckPoint = _newPeckPoint;
            _newPeckPoint = null;

            _peckCoroutine = StartCoroutine(PeckRoutine());
        }

        public void Scare(Transform danger)
        {
            SetMoveSpeed(MoveSpeed.Rush);

            if (_scareCoroutine != null)
                return;

            _isScared = true;

            if (_goToNewPeckPointCoroutine != null)
            {
                StopCoroutine(_goToNewPeckPointCoroutine);
                _goToNewPeckPointCoroutine = null;
            }

            if (_peckCoroutine != null)
            {
                StopCoroutine(_peckCoroutine);
                _peckCoroutine = null;
            }

            _scareCoroutine = StartCoroutine(ScaredRushRoutine(danger));
        }

        public void Unscare()
        {
            _isScared = false;

            if (_scareCoroutine != null)
            {
                StopCoroutine(_scareCoroutine);
                _scareCoroutine = null;
            }

            _animator.SetBool(WalkBool, false);

            if (_newPeckPoint != null)
            {
                _goToNewPeckPointCoroutine = StartCoroutine(GoToNewPeckPointRoutine(beScared: true));
            }
            else
            {
                float currentDistance = Vector3.Distance(_currentPeckPoint.transform.position, transform.position);

                if (currentDistance > _peckPointRadius)
                {
                    _returnToPeckPointCoroutine = StartCoroutine(ReturnToPeckPointRoutine());
                }
                else
                {
                    _peckCoroutine = StartCoroutine(PeckRoutine());
                }
            }
        }

        private IEnumerator ReturnToPeckPointRoutine()
        {
            SetMoveSpeed(MoveSpeed.Rush);

            if (_obstacle.enabled)
            {
                _obstacle.enabled = false;
                yield return new WaitForSeconds(MIN_DELAY_CARVE_SWITCH_OFF);
            }

            if (!_agent.enabled)
                _agent.enabled = true;

            _agent.SetDestination(_currentPeckPoint.transform.position);
            _animator.SetBool(WalkBool, true);

            yield return new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);

            _animator.SetBool(WalkBool, false);

            _peckCoroutine = StartCoroutine(PeckRoutine());
        }

        private IEnumerator ScaredRushRoutine(Transform danger)
        {
            NavMeshPath path = new();

            if (_obstacle.enabled)
            {
                _obstacle.enabled = false;
                yield return new WaitForSeconds(MIN_DELAY_CARVE_SWITCH_OFF);
            }

            if (!_agent.enabled)
                _agent.enabled = true;

            while (_isScared)
            {
                bool pointFound = false;

                Vector3 directionFromDanger = (transform.position - danger.position).normalized;

                yield return StartCoroutine(TryFindRushPointRoutine(directionFromDanger, path, isFound => pointFound = isFound));

                if (pointFound)
                {
                    Vector3 pointToRun = path.corners.Last();

                    _animator.SetBool(WalkBool, true);
                    _agent.SetDestination(pointToRun);

                    yield return new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);

                    _animator.SetBool(WalkBool, false);
                }
                else
                {
                    Debug.LogWarningFormat(LogStr.WARNING_CANT_FIND_PATH, gameObject.name, transform.position);
                }
            }
        }

        private IEnumerator TryFindRushPointRoutine(Vector3 direction, NavMeshPath path, Action<bool> callback)
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
            Vector3 targetPoint = origin + rotatedDir * _escapeDistance;

            if (NavMeshUtils.TryGetRandomPointAtDistance(origin, targetPoint, _searchRadius, path, _attemptsForSearch))
            {
                return true;
            }

            return false;
        }

        private void OnDisable()
        {
            _isAlive = false;

            if (_peckCoroutine != null)
            {
                StopCoroutine(_peckCoroutine);
                _peckCoroutine = null;
            }

            if (_goToNewPeckPointCoroutine != null)
            {
                StopCoroutine(_goToNewPeckPointCoroutine);
                _goToNewPeckPointCoroutine = null;
            }

            if (_scareCoroutine != null)
            {
                StopCoroutine(_scareCoroutine);
                _scareCoroutine = null;
            }

            if (_returnToPeckPointCoroutine != null)
            {
                StopCoroutine(_returnToPeckPointCoroutine);
                _returnToPeckPointCoroutine = null;
            }
        }
    }
}