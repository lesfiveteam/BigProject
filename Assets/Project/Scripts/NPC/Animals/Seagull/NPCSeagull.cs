using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
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

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _body;
        [SerializeField] private EnvironmentSound _environmentSound;

        [SerializeField] private float _flySpeed = 6f;
        [SerializeField] private float _rotationSpeed = 50f;
        [SerializeField] private int _waypointsRootCount = 3;

        private float _takeoffDistance = 20f;
        private float _flyHeight = 120f;
        private float _waypointRadius = 15f;
        private float _landingPointGenerationDistance = 40f;

        private Coroutine _flightCoroutine;
        private List<Coroutine> _coroutines = new();
        private Transform[] _waypoints;
        private AnimationClip _clipJump;

        private Vector3 _startPoint;
        private Vector3 _startRotation;

        private Vector3 _defaultScale;
        private Vector3 _flyScale;
        private float _flyScaleModify = 0.7f;

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
            _flyScale = _defaultScale * _flyScaleModify;

            _clipJump = _animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == JUMP_CLIP_NAME);

            _coroutines.Add(StartCoroutine(BorringTimeRoutine()));
        }

        private IEnumerator BorringTimeRoutine()
        {
            while (_animator != null)
            {
                float boredTime = Random.Range(3f, 7f);
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
            InFly = false;
        }

        public void StartFlight(Vector3 direction)
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                GameLogManager.Warning(LogStr.WARNING_EMPTY_COLLECTION);
                return;
            }

            if (_flightCoroutine != null)
                StopCoroutine(_flightCoroutine);

            direction.y = 0;
            _flightCoroutine = StartCoroutine(FlightRoutine(direction));
            _coroutines.Add(_flightCoroutine);
        }

        private IEnumerator FlightRoutine(Vector3 startDirection)
        {
            InFly = true;
            Coroutine coroutine;

            // Take off
            _environmentSound.PlaySound();
            Vector3 takeoffTarget = GetAirPoint(_startPoint) + startDirection * _takeoffDistance;
            coroutine = StartCoroutine(TakeoffRoutine(takeoffTarget));
            _coroutines.Add(coroutine);
            yield return coroutine;
            _coroutines.Remove(coroutine);

            // Fly the root
            List<Vector3> airPoints = GetAirPoints();

            if (airPoints.Count > 0)
            {
                for (int i = 0; i < airPoints.Count; i++)
                {
                    coroutine = StartCoroutine(FlyToPointRoutine(airPoints[i]));
                    _coroutines.Add(coroutine);
                    yield return coroutine;
                    _coroutines.Remove(coroutine);
                }
            }

            // Go home
            coroutine = StartCoroutine(FlyToHomeRoutine(GetAirPoint(_startPoint)));
            _coroutines.Add(coroutine);
            yield return coroutine;
            _coroutines.Remove(coroutine);

            // Landing
            _environmentSound.PlaySound();
            coroutine = StartCoroutine(LandRoutine());
            _coroutines.Add(coroutine);
            yield return coroutine;
            _coroutines.Remove(coroutine);

            // End flight
            InFly = false;
            _flightCoroutine = null;
        }

        private IEnumerator TakeoffRoutine(Vector3 target)
        {
            _animator.SetTrigger(JumpTrigger);
            _animator.SetFloat(FlySpeedModifier, HIGH_FLY_SPEED_MODIFIER);

            Vector3 pointToTakeOff = target;
            pointToTakeOff.y -= 5f;

            float distanceForScale = Vector3.Distance(transform.position, pointToTakeOff) / 1.2f;

            float frameDuration = _clipJump.length / _clipJump.frameRate;
            float timeForJumpFrames = frameDuration * JUMP_FRAMES;

            float minDistance = 0.5f;
            float currentDistance;

            yield return new WaitForSeconds(timeForJumpFrames);

            do
            {
                Vector3 direction = (pointToTakeOff - transform.position).normalized;

                Move(_flySpeed, direction);

                yield return new WaitWhile(() => Time.timeScale == 0);

                currentDistance = Vector3.Distance(transform.position, pointToTakeOff);

                if (currentDistance < distanceForScale)
                {
                    float progress = 1f - currentDistance / distanceForScale;
                    transform.localScale = Vector3.Lerp(_defaultScale, _flyScale, progress * progress);
                }
            }
            while (currentDistance > minDistance);

            transform.position = pointToTakeOff;
        }

        private IEnumerator FlyToPointRoutine(Vector3 target)
        {
            _animator.SetFloat(FlySpeedModifier, LOW_FLY_SPEED_MODIFIER);

            bool passed = false;

            while (!passed)
            {
                Vector3 direction = (target - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, target);

                if (distanceToTarget < _waypointRadius && Vector3.Dot(transform.forward, direction) < 0)
                {
                    passed = true;
                    break;
                }

                Move(_flySpeed, direction);

                yield return new WaitWhile(() => Time.timeScale == 0);
            }
        }

        private IEnumerator FlyToHomeRoutine(Vector3 target)
        {
            bool passed = false;
            bool landingPointGenerated = false;

            while (!passed)
            {
                Vector3 direction = (target - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, target);

                if (!landingPointGenerated && distanceToTarget < _landingPointGenerationDistance)
                {
                    Vector3 landingOffsetedPoint = (transform.position - _startPoint).normalized * _takeoffDistance;
                    landingOffsetedPoint.y = 0;
                    target += landingOffsetedPoint;

                    landingPointGenerated = true;
                }

                if (distanceToTarget < _waypointRadius && Vector3.Dot(transform.forward, direction) < 0)
                {
                    passed = true;
                    break;
                }

                Move(_flySpeed, direction);

                yield return new WaitWhile(() => Time.timeScale == 0);
            }
        }

        private IEnumerator LandRoutine()
        {
            _animator.SetFloat(FlySpeedModifier, HIGH_FLY_SPEED_MODIFIER);

            Quaternion defaultAngle = _body.localRotation;
            Quaternion landingAggle = Quaternion.Euler(new Vector3(_body.localEulerAngles.x, _body.localEulerAngles.y, _body.localEulerAngles.z - 90));
            float angleSpeed = 30;

            Vector3 start = transform.position;
            Vector3 target = _startPoint;
            bool finished = false;

            float distanceForScale = Vector3.Distance(transform.position, target) / 1.2f;
            float currentDistance;

            float currentSpeed = _flySpeed;
            float minDistance = 0.5f;
            float minSpeed = _flySpeed / 4;

            while (!finished)
            {
                Vector3 direction = (target - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, target);

                _body.localRotation = Quaternion.RotateTowards(_body.localRotation, landingAggle, angleSpeed * Time.deltaTime);

                currentDistance = Vector3.Distance(transform.position, target);

                if (currentDistance < distanceForScale)
                {
                    float progress = 1f - currentDistance / distanceForScale;
                    transform.localScale = Vector3.Lerp(_flyScale, _defaultScale, progress * progress);
                }

                if (distanceToTarget < minDistance && Vector3.Dot(transform.forward, direction) < 0)
                {
                    transform.position = target;
                    transform.localScale = _defaultScale;
                    finished = true;
                    break;
                }

                float t = minDistance / distanceToTarget;
                currentSpeed = Mathf.Lerp(_flySpeed, minSpeed, t);

                Move(currentSpeed, direction);

                yield return new WaitWhile(() => Time.timeScale == 0);
            }

            _animator.SetTrigger(JumpBackTrigger);

            _body.localRotation = defaultAngle;
            transform.position = target;
            transform.eulerAngles = _startRotation;
        }

        private List<Vector3> GetAirPoints()
        {
            List<Vector3> result = new();
            List<Transform> availabeWaypoint = new(_waypoints);

            int waypointsCount = Mathf.Min(_waypointsRootCount, availabeWaypoint.Count);

            for (int i = 0; i < waypointsCount; i++)
            {
                int index = Random.Range(0, availabeWaypoint.Count);
                result.Add(GetAirPoint(availabeWaypoint[index].position));
                availabeWaypoint.RemoveAt(index);
            }

            return result;
        }

        private Vector3 GetAirPoint(Vector3 groundPoint)
            => new(groundPoint.x, _flyHeight, groundPoint.z);

        private void Move(float moveSpeed, Vector3 direction)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
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

            if (_coroutines.Count > 0)
            {
                foreach (Coroutine flightCoroutine in _coroutines)
                {
                    if (flightCoroutine != null)
                        StopCoroutine(flightCoroutine);
                }

                _coroutines.Clear();
            }
        }

        private void OnDestroy()
        {
            StopFlight();
        }
    }
}