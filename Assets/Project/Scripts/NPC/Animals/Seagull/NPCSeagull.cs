using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.Sound;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Project.Scripts.NPC.Animals.Seagull
{
    public class NPCSeagull : MonoBehaviour, IScared
    {
        public Action<NPCSeagull, bool> ReadyToFly;

        private readonly int BoredTrigger = Animator.StringToHash("Bored");
        private readonly int JumpTrigger = Animator.StringToHash("Jump");
        private readonly int JumpBackTrigger = Animator.StringToHash("JumpBack");
        private readonly int FlySpeedModifier = Animator.StringToHash("flySpeed");
        private const string JUMP_CLIP_NAME = "Animation_Jump";
        private const int JUMP_FRAMES = 8;
        private const float HIGH_FLY_SPEED_MODIFIER = 3.0f;
        private const float LOW_FLY_SPEED_MODIFIER = 0.3f;

        private const int WAYPOINTS_ROOT_COUNT = 3;
        private const float FLY_SCALE_MODIFY = 0.7f;
        private const float FLY_SPEED = 6f;
        private const float ROTATION_SPEED = 50f;
        private const float TAKEOFF_DISTANCE = 20f;
        private const float FLY_HEIGHT = 120f;
        private const float WAYPOINT_RADIUS = 15f;
        private const float LANDING_POINT_GENERATION_DISTANCE = 40f;
        private const float MIN_BORED_TIME = 3f;
        private const float MAX_BORED_TIME = 7f;

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _body;
        [SerializeField] private EnvironmentSound _environmentSound;

        private Transform[] _waypoints;
        private Coroutine _flightCoroutine;
        private Coroutine _boringCoroutines;

        private WaitForSeconds _jumpDelay;
        private WaitWhile _pausedCondition;

        private Vector3 _startPoint;
        private Vector3 _startRotation;

        private Vector3 _defaultScale;
        private Vector3 _flyScale;

        private bool _inFly;
        public bool InFly
        {
            get => _inFly;
            private set
            {
                _inFly = value;
                ReadyToFly?.Invoke(this, !_inFly);
            }
        }

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);
            ExceptionUtilities.ThrowIfNullFormat(_body);
            ExceptionUtilities.ThrowIfNullFormat(_environmentSound);

            _startPoint = transform.position;
            _startRotation = transform.eulerAngles;

            _defaultScale = transform.localScale;
            _flyScale = _defaultScale * FLY_SCALE_MODIFY;

            AnimationClip clipJump = _animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == JUMP_CLIP_NAME);
            float jumpFrameDuration = clipJump.length / clipJump.frameRate;
            float timeForJumpFrames = jumpFrameDuration * JUMP_FRAMES;
            _jumpDelay = new WaitForSeconds(timeForJumpFrames);

            _pausedCondition = new WaitWhile(() => Time.timeScale == 0);

            _boringCoroutines = StartCoroutine(BorringTimeRoutine());
        }

        private IEnumerator BorringTimeRoutine()
        {
            float boredTime;

            while (_animator != null)
            {
                if (InFly)
                {
                    yield return _pausedCondition;
                    continue;
                }

                boredTime = Random.Range(MIN_BORED_TIME, MAX_BORED_TIME);

                yield return new WaitForSeconds(boredTime);

                if (!InFly)
                    _animator.SetTrigger(BoredTrigger);
            }
        }

        public void Scare(Transform danger)
        {
            if (InFly)
                return;

            Vector3 direction = (transform.position - danger.position).normalized;
            StartFlight(direction);
        }

        public void SetWaypoints(Transform[] waypoints)
        {
            _waypoints = waypoints;
        }

        public void StartFlight(Vector3 direction)
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                GameLogManager.Warning(LogStr.WARNING_EMPTY_COLLECTION);
                return;
            }

            direction.y = 0;

            if (_flightCoroutine != null)
                StopCoroutine(_flightCoroutine);

            _flightCoroutine = StartCoroutine(FlightRoutine(direction));
        }

        private IEnumerator FlightRoutine(Vector3 startDirection)
        {
            InFly = true;

            // Take off
            _environmentSound.PlaySound();
            Vector3 takeoffPoint = GetAirPoint(_startPoint) + startDirection * TAKEOFF_DISTANCE;
            yield return TakeoffRoutine(takeoffPoint);

            // Fly the root
            List<Vector3> airPoints = GetAirPoints();

            if (airPoints.Count > 0)
            {
                for (int i = 0; i < airPoints.Count; i++)
                {
                    yield return FlyToPointRoutine(airPoints[i]);
                }
            }
            else
            {
                Debug.LogWarningFormat(LogStr.WARNING_EMPTY_COLLECTION);
            }

            // Go home
            yield return FlyToHomeRoutine(GetAirPoint(_startPoint));

            // Landing
            _environmentSound.PlaySound();
            yield return LandRoutine();

            // End flight
            InFly = false;
            _flightCoroutine = null;
        }

        private IEnumerator TakeoffRoutine(Vector3 takeoffPoint)
        {
            _animator.SetTrigger(JumpTrigger);
            _animator.SetFloat(FlySpeedModifier, HIGH_FLY_SPEED_MODIFIER);

            float accelerationСompensation = 5f;
            takeoffPoint.y -= accelerationСompensation;

            float pathFraction = 0.8f;
            float distanceForScale = Vector3.Distance(transform.position, takeoffPoint) * pathFraction;

            float minDistance = 0.5f;
            float currentDistance;

            float progress;

            Vector3 direction = (takeoffPoint - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);

            yield return _jumpDelay;

            do
            {
                direction = (takeoffPoint - transform.position).normalized;
                Move(FLY_SPEED, direction);

                yield return _pausedCondition;

                currentDistance = Vector3.Distance(transform.position, takeoffPoint);

                if (currentDistance < distanceForScale)
                {
                    progress = 1f - currentDistance / distanceForScale;
                    transform.localScale = Vector3.Lerp(_defaultScale, _flyScale, progress * progress);
                }
            }
            while (currentDistance > minDistance);

            transform.position = takeoffPoint;
        }

        private IEnumerator FlyToPointRoutine(Vector3 target)
        {
            _animator.SetFloat(FlySpeedModifier, LOW_FLY_SPEED_MODIFIER);

            Vector3 direction;
            float distanceToTarget;
            bool passed = false;

            while (!passed)
            {
                direction = (target - transform.position).normalized;
                distanceToTarget = Vector3.Distance(transform.position, target);

                Move(FLY_SPEED, direction);

                if (distanceToTarget < WAYPOINT_RADIUS 
                    && Vector3.Dot(transform.forward, direction) < 0)
                {
                    passed = true;
                }

                yield return _pausedCondition;
            }
        }

        private IEnumerator FlyToHomeRoutine(Vector3 target)
        {
            bool passed = false;
            bool landingPointGenerated = false;

            Vector3 direction;
            float distanceToTarget;

            while (!passed)
            {
                distanceToTarget = Vector3.Distance(transform.position, target);

                if (!landingPointGenerated && distanceToTarget < LANDING_POINT_GENERATION_DISTANCE)
                {
                    Vector3 landingOffsetedPoint = (transform.position - _startPoint).normalized * TAKEOFF_DISTANCE;
                    landingOffsetedPoint.y = 0;
                    target += landingOffsetedPoint;

                    landingPointGenerated = true;
                }

                direction = (target - transform.position).normalized;
                
                Move(FLY_SPEED, direction);

                if (distanceToTarget < WAYPOINT_RADIUS && Vector3.Dot(transform.forward, direction) < 0)
                {
                    passed = true;
                }

                yield return _pausedCondition;
            }
        }

        private IEnumerator LandRoutine()
        {
            _animator.SetFloat(FlySpeedModifier, HIGH_FLY_SPEED_MODIFIER);

            Quaternion defaultAngle = _body.localRotation;
            Quaternion landingAggle = Quaternion.Euler(new Vector3(_body.localEulerAngles.x, _body.localEulerAngles.y, _body.localEulerAngles.z - 90));
            float angleSpeed = 30;

            Vector3 direction;
            float distanceToTarget;
            float progress;
            float time;

            bool finished = false;
            bool isTargetAngelReached = false;

            float pathFraction = 0.8f;
            float distanceForScale = Vector3.Distance(transform.position, _startPoint) * pathFraction;

            float currentSpeed;
            float minDistance = 0.5f;
            float minAngle = 0.1f;
            float minSpeed = FLY_SPEED / 4;

            while (!finished)
            {
                if (!isTargetAngelReached)
                {
                    if (Quaternion.Angle(_body.localRotation, landingAggle) >= minAngle)
                    {
                        _body.localRotation = Quaternion.RotateTowards(_body.localRotation, landingAggle, angleSpeed * Time.deltaTime);
                    }
                    else
                    {
                        isTargetAngelReached = true;
                    }
                }

                distanceToTarget = Vector3.Distance(transform.position, _startPoint);

                if (distanceToTarget < distanceForScale)
                {
                    progress = 1f - distanceToTarget / distanceForScale;
                    transform.localScale = Vector3.Lerp(_flyScale, _defaultScale, progress * progress);
                }

                time = minDistance / distanceToTarget;
                currentSpeed = Mathf.Lerp(FLY_SPEED, minSpeed, time);

                direction = (_startPoint - transform.position).normalized;

                Move(currentSpeed, direction);

                if (distanceToTarget < minDistance && Vector3.Dot(transform.forward, direction) < 0)
                {
                    transform.position = _startPoint;
                    transform.localScale = _defaultScale;
                    finished = true;
                }

                yield return _pausedCondition;
            }

            _animator.SetTrigger(JumpBackTrigger);

            _body.localRotation = defaultAngle;
            transform.position = _startPoint;
            transform.eulerAngles = _startRotation;
        }

        private List<Vector3> GetAirPoints()
        {
            List<Vector3> result = new();
            List<Transform> availabeWaypoint = new(_waypoints);

            int waypointsCount = Mathf.Min(WAYPOINTS_ROOT_COUNT, availabeWaypoint.Count);

            for (int i = 0; i < waypointsCount; i++)
            {
                int index = Random.Range(0, availabeWaypoint.Count);
                result.Add(GetAirPoint(availabeWaypoint[index].position));
                availabeWaypoint.RemoveAt(index);
            }

            return result;
        }

        private Vector3 GetAirPoint(Vector3 groundPoint)
            => new(groundPoint.x, FLY_HEIGHT, groundPoint.z);

        private void Move(float moveSpeed, Vector3 direction)
        {
            transform.position += moveSpeed * Time.deltaTime * transform.forward;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, ROTATION_SPEED * Time.deltaTime);
            }
        }

        public void StopFlight()
        {
            InFly = false;

            if (_flightCoroutine != null)
            {
                StopCoroutine(_flightCoroutine);
                _flightCoroutine = null;
            }

            if (_boringCoroutines != null)
            {
                StopCoroutine(_boringCoroutines);
                _boringCoroutines = null;
            }
        }

        private void OnDisable()
        {
            StopFlight();
        }
    }
}