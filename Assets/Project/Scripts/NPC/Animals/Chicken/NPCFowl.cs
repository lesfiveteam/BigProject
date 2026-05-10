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
        private const int ATTEMPTS_FOR_SEARCH = 10;
        private const float PECK_POINT_RADIUS = 5f;
        private const float PECK_RADIUS = 4f;
        private const float MIN_PECK_TIME = 2f;
        private const float MAX_PECK_TIME = 5f;
        private const float ANGLE_STEP = 2f;
        private const float ESCAPE_DISTANCE = 3f;
        private const float SEARCH_RADIUS = 1f;

        private readonly int _speedFloat = Animator.StringToHash("Speed");
        private readonly int _walkBool = Animator.StringToHash("isWalk");

        [SerializeField] protected Animator _animator;
        [SerializeField] protected NavMeshAgent _agent;

        protected NPCPeckPoint _currentPeckPoint;
        protected NPCPeckPoint _newPeckPoint;
        protected Vector3 _pointToMove;

        protected Coroutine _peckCoroutine;
        protected Coroutine _goToNewPeckPointCoroutine;
        protected Coroutine _scareCoroutine;
        protected Coroutine _returnToPeckPointCoroutine;

        protected WaitWhile _cachedWaitWhilePause = new(() => Time.timeScale == 0);
        private WaitForSeconds _cachedChickenDelayTime = new(2f);
        private WaitForSeconds _cachedPeckTime;
        private WaitUntil _cachedArrivedCondition;
        
        protected bool _isAlive = false;
        protected bool _isScared = false;

        private enum MoveSpeed
        {
            Walk,
            Run,
            Rush,
        }

        protected virtual void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_agent);
            ExceptionUtilities.ThrowIfNullFormat(_animator);

            float currentPeckTime = Random.Range(MIN_PECK_TIME, MAX_PECK_TIME);
            _cachedPeckTime = new WaitForSeconds(currentPeckTime);
            _cachedArrivedCondition = new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);
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

            _animator.SetBool(_walkBool, false);

            if (_newPeckPoint != null)
            {
                _goToNewPeckPointCoroutine = StartCoroutine(GoToNewPeckPointRoutine(beScared: true));
            }
            else
            {
                float currentDistance = Vector3.Distance(_currentPeckPoint.transform.position, transform.position);

                if (currentDistance > PECK_POINT_RADIUS)
                {
                    _returnToPeckPointCoroutine = StartCoroutine(ReturnToPeckPointRoutine());
                }
                else
                {
                    _peckCoroutine = StartCoroutine(PeckRoutine());
                }
            }
        }

        protected IEnumerator PeckRoutine()
        {
            SetMoveSpeed(MoveSpeed.Walk);

            NavMeshPath path = new();

            float currentPeckRadius = PECK_RADIUS;
            bool isFounded = false;

            // PeckTimer(); // ToDo: need to check logic and fix

            while (_isAlive)
            {
                if (_isScared)
                {
                    yield return _cachedWaitWhilePause;
                    continue;
                }

                yield return _cachedPeckTime;

                yield return NavMeshUtils.TryGetRandomPointInCircleRoutine(
                    transform.position,
                    transform.position,
                    currentPeckRadius,
                    _currentPeckPoint.transform.position,
                    PECK_POINT_RADIUS,
                    path,
                    (founded) => isFounded = founded);

                if (isFounded)
                {
                    currentPeckRadius = PECK_RADIUS;
                    Vector3 newPeckTarget = path.corners.Last();

                    _agent.SetDestination(newPeckTarget);

                    _animator.SetBool(_walkBool, true);

                    yield return _cachedArrivedCondition;

                    _animator.SetBool(_walkBool, false);
                }
                else
                {
                    currentPeckRadius++;
                }
            }
        }

        protected virtual void PeckTimer() { }

        protected IEnumerator GoToNewPeckPointRoutine(bool isChicken = false, bool beScared = false)
        {
            if (isChicken)
                yield return _cachedChickenDelayTime;

            if (_peckCoroutine != null)
            {
                StopCoroutine(_peckCoroutine);
                _peckCoroutine = null;
            }

            SetMoveSpeed(MoveSpeed.Run);

            NavMeshPath path = new();

            if (beScared)
            {
                _pointToMove = _newPeckPoint.transform.position;

                yield return NavMeshUtils.TryGetRandomPointInCircleRoutine(
                    transform.position,
                    _pointToMove,
                    PECK_POINT_RADIUS,
                    _newPeckPoint.transform.position,
                    PECK_POINT_RADIUS,
                    path);
            }
            else
            {
                Vector3 currentOffset = transform.position - _currentPeckPoint.transform.position;
                _pointToMove = _newPeckPoint.transform.position + currentOffset;

                float searchRadius = PECK_POINT_RADIUS - currentOffset.magnitude;

                bool isFounded = false;

                yield return NavMeshUtils.TryGetRandomPointInCircleRoutine(
                    transform.position,
                    _pointToMove,
                    searchRadius,
                    _newPeckPoint.transform.position,
                    PECK_POINT_RADIUS,
                    path,
                    (founded) => isFounded = founded);

                while (!isFounded)
                {
                    searchRadius++;

                    yield return NavMeshUtils.TryGetRandomPointInCircleRoutine(
                    transform.position,
                    _pointToMove,
                    searchRadius,
                    _newPeckPoint.transform.position,
                    PECK_POINT_RADIUS,
                    path,
                    (founded) => isFounded = founded);
                }
            }

            if (path == null || path.corners.Length <= 1)
                Debug.LogWarningFormat(LogStr.WARNING_CANT_FIND_PATH, gameObject.name, transform.position);

            _agent.SetDestination(path.corners.Last());
            _animator.SetBool(_walkBool, true);

            yield return _cachedArrivedCondition;

            _animator.SetBool(_walkBool, false);

            _pointToMove = Vector3.zero;
            _currentPeckPoint = _newPeckPoint;
            _newPeckPoint = null;

            _peckCoroutine = StartCoroutine(PeckRoutine());
        }

        private void SetMoveSpeed(MoveSpeed speed)
        {
            float modifier = speed switch
            {
                MoveSpeed.Walk => 1f,
                MoveSpeed.Run => 3f,
                MoveSpeed.Rush => 5f,
                _ => 1f
            };

            _animator.SetFloat(_speedFloat, modifier);
            _agent.acceleration = modifier;
        }

        private IEnumerator ReturnToPeckPointRoutine()
        {
            SetMoveSpeed(MoveSpeed.Rush);

            _agent.SetDestination(_currentPeckPoint.transform.position);
            _animator.SetBool(_walkBool, true);

            yield return _cachedArrivedCondition;

            _animator.SetBool(_walkBool, false);

            _peckCoroutine = StartCoroutine(PeckRoutine());
        }

        private IEnumerator ScaredRushRoutine(Transform danger)
        {
            NavMeshPath path = new();

            while (_isScared)
            {
                bool pointFound = false;

                Vector3 directionFromDanger = (transform.position - danger.position).normalized;

                yield return TryFindRushPointRoutine(
                    directionFromDanger,
                    path,
                    isFound => pointFound = isFound);

                if (pointFound)
                {
                    Vector3 pointToRun = path.corners.Last();

                    _animator.SetBool(_walkBool, true);
                    _agent.SetDestination(pointToRun);

                    yield return _cachedArrivedCondition;

                    _animator.SetBool(_walkBool, false);
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
            Vector3 rotatedDir;
            Vector3 targetPoint;
            bool isFounded = false;

            for (float angle = 0; angle <= 180; angle += ANGLE_STEP)
            {
                rotatedDir = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                targetPoint = origin + rotatedDir * ESCAPE_DISTANCE;

                yield return NavMeshUtils.TryGetRandomPointAtDistanceRoutine(
                    origin,
                    targetPoint,
                    SEARCH_RADIUS,
                    path,
                    (founded) => isFounded = founded,
                    ATTEMPTS_FOR_SEARCH);

                if (isFounded && path.corners.Length == 2)
                {
                    callback?.Invoke(true);
                    yield break;
                }

                attemptsCount++;

                rotatedDir = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                targetPoint = origin + rotatedDir * ESCAPE_DISTANCE;

                yield return NavMeshUtils.TryGetRandomPointAtDistanceRoutine(
                    origin,
                    targetPoint,
                    SEARCH_RADIUS,
                    path,
                    (founded) => isFounded = founded,
                    ATTEMPTS_FOR_SEARCH);

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