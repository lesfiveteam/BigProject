using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Intercatable;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Systems.Sound;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace BigProject.Player
{
    public class PlayerController : MonoBehaviour
    {
        public Action OnUp;
        public Action OnDown;

        private const float MIN_WALK_SPEED_COEFF = 0.1f;
        private const float MIN_ANIM_SPEED_COEFF = 0.5f;
        private const float MAX_TARGET_ANGLE_OFFSET = 3f;
        private const float JUMP_VOLUME = 0.2f;

        private const float LINK_SPEED = 4f;
        private const float JUMP_SPEED = 7f;
        private const int JUMP_LINK_ID = 2;
        private const int CLIMB_LINK_ID = 3;
        private const int JUMP_START_FRAMES = 15;
        private const int JUMP_FINISH_FRAMES = 45;
        private const string JUMP_CLIP_NAME = "Jump";
        private readonly int JumpTrigger = Animator.StringToHash("Jump");
        private readonly int JumpAnimationSpeed = Animator.StringToHash("JumpSpeed");
        private readonly int IsMoving = Animator.StringToHash("IsMoving");
        private readonly int IsBoredAnimationBool = Animator.StringToHash("IsBored");

        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Animator _animatorController;
        [SerializeField] private GameObject _clickEffectSplash;
        [SerializeField] private AudioClip _jumpDone;

        [SerializeField] private float _navMeshHitPointDistance = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private bool _walkOnHolding = true;
        [SerializeField] private float _walkOnHoldingCheckInterval = 0.2f;
        [SerializeField] private float _timeToBored = 7f;

        private Vector3 _lastClickPosition;

        private PlayerInputHandler _inputHandler;
        private IInteractable _interactable = null;
        private Vector3 _destination;
        private bool _isMoving;

        private bool _isBored;
        private bool IsBored
        {
            get => _isBored;
            set
            {
                _isBored = value;
                _animatorController.SetBool(IsBoredAnimationBool, value);
            }
        }

        private Coroutine _linkProcess;
        private AnimationClip _clipJump;

        private Camera _camera;
        private SceneLoadManager _sceneLoader;
        private SoundsManager _soundsManager;
        private Coroutine _walkOnHoldingCoroutine;
        private WaitForSeconds _walkOnHolingCheckWait;
        private Coroutine _interactCoroutine;
        private Coroutine _autopilotCoroutine;
        private float _animSpeed;
        private int _layerMask;
        private float _boredTimer;
        private WaitWhile _cachedWaitWhilePause = new(() => Time.timeScale == 0);

        public bool IsAutopilot { private set; get;}

        public void Init(PlayerInputHandler inputHandler, SceneLoadManager sceneLoader, SoundsManager soundsManager)
        {
            _inputHandler = inputHandler;
            _sceneLoader = sceneLoader;
            _soundsManager = soundsManager;

            ExceptionUtilities.ThrowIfNull(_inputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerInputHandler"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SoundsManager"));

            _animSpeed = _animatorController.speed;
            _clipJump = _animatorController.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == JUMP_CLIP_NAME);
            _layerMask = ~((1 << LayerMask.NameToLayer("Scared")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));

            _navMeshAgent.updateRotation = false;

            _walkOnHolingCheckWait = new(_walkOnHoldingCheckInterval);
        }

        public void AutoTarget(IInteractable interactable)
        {
            if (!_navMeshAgent.isOnNavMesh)
            {
                if (_autopilotCoroutine != null)
                {
                    StopCoroutine(_autopilotCoroutine);
                    _autopilotCoroutine = null;
                }

                _autopilotCoroutine = StartCoroutine(GameplayUtilities.DoAfterConditionRoutine
                    (() => _navMeshAgent.isOnNavMesh, () => SetAutoTarget(interactable)));
            }
            else
            {
                SetAutoTarget(interactable);
            }
        }

        /// <summary>
        /// Creates sound of walking
        /// Make sure that collider of the ground is lower, than the boy transform point, or else the raycast will cast incorrectrly, ignoring the floor
        /// </summary>
        public void PlayGroundSound()
        {
            Ray ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.TryGetComponent(out GroundSounds groundSounds))
                {
                    _soundsManager.PlaySound(groundSounds.GetStepSound(), volume: groundSounds.StepVolume, spawnPosition: transform);
                }
            }
        }

        private void Start()
        {
            FindCamera();
        }

        private void OnEnable()
        {
            _inputHandler.Click += OnClick;
            _inputHandler.ClickRelease += OnClickRelease;
            _sceneLoader.SceneLoadingCompleted += OnSceneLoadingCompleted;
        }

        private void OnDisable()
        {
            _inputHandler.Click -= OnClick;
            _inputHandler.ClickRelease -= OnClickRelease;
            _sceneLoader.SceneLoadingCompleted -= OnSceneLoadingCompleted;
        }

        private void OnSceneLoadingCompleted() => FindCamera();

        private void FindCamera()
        {
            _camera = GetComponent<Camera>();

            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnClick()
        {
            if (!IsAutopilot && !GameplayUtilities.IsPointerOverUI())
            {
                _walkOnHoldingCoroutine = StartCoroutine(CalculateMovementRoutine());
            }
        }

        private void OnClickRelease()
        {
            if (_walkOnHoldingCoroutine != null)
            {
                StopCoroutine(_walkOnHoldingCoroutine);
                _walkOnHoldingCoroutine = null;
            }
        }

        private void Update()
        {
            if (TryLink())
                return;

            if (_isMoving)
            {
                StopBoring();
                RotateTowardsMovement();
                HandleWalkAnimation();

                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance &&
                    (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f))
                {
                    _navMeshAgent.ResetPath(); // fix of rare levitation
                    _isMoving = false;
                    _animatorController.SetBool(IsMoving, false);
                    TryInteract();
                }
            }
            else
            {
                if(!IsBored)
                    Boring();
            }
        }

        private void Boring()
        {
            _boredTimer += Time.deltaTime;

            if (_boredTimer > _timeToBored)
            {
                _boredTimer = 0;
                IsBored = true;
            }
        }

        private void StopBoring()
        {
            _boredTimer = 0;
            IsBored = false;
        }

        private IEnumerator CalculateMovementRoutine()
        {
            bool isFirstInteration = true;

            do
            {
                Vector2 mousePosition = _inputHandler.GetMousePosition();
                Ray ray = _camera.ScreenPointToRay(mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
                {
                    if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, _navMeshHitPointDistance, NavMesh.AllAreas))
                    {
                        IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();
                        SetInterableObject(interactableObject);

                        if (TryInteract())
                        {
                            break;
                        }

                        SetDestination(navMeshHit.position, interactableObject == null && isFirstInteration);
                        isFirstInteration = false;
                        Move();
                    }
                }

                yield return _walkOnHolingCheckWait;
            } while (_walkOnHolding);
        }

        private void RotateTowardsMovement()
        {
            if (_navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
            {
                // Getting the move direction
                Vector3 moveDirection = _navMeshAgent.velocity.normalized;
                moveDirection.y = 0;

                if (moveDirection != Vector3.zero)
                {
                    // Creating quaternion towards move direction
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                    // Smoothly rotating the player
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        _rotationSpeed * Time.deltaTime
                    );
                }
            }
        }

        private void HandleWalkAnimation()
        {
            float speedCoeff = _navMeshAgent.velocity.magnitude / _navMeshAgent.speed;
            bool isMovingAnim = speedCoeff > MIN_WALK_SPEED_COEFF;
            _animatorController.SetBool(IsMoving, isMovingAnim);

            if (isMovingAnim)
            {
                _animatorController.speed = _animSpeed * (MIN_ANIM_SPEED_COEFF + speedCoeff * (1f - MIN_ANIM_SPEED_COEFF));
            }
        }

        private void Move()
        {
            _isMoving = true;
            _navMeshAgent.SetDestination(_destination);
        }

        private bool TryLink()
        {
            if (_navMeshAgent.isOnOffMeshLink)
            {
                if (_linkProcess == null)
                {
                    _isMoving = true;

                    StopBoring();

                    int linkArea = (_navMeshAgent.currentOffMeshLinkData.owner as NavMeshLink)?.area ?? JUMP_LINK_ID;

                    switch (linkArea)
                    {
                        case JUMP_LINK_ID:
                            _linkProcess = StartCoroutine(JumpProcess(_navMeshAgent.currentOffMeshLinkData));
                            break;

                        case CLIMB_LINK_ID:
                            _linkProcess = StartCoroutine(ClimbProcess(_navMeshAgent.currentOffMeshLinkData));
                            break;

                        default:
                            throw new ArgumentException(string.Format(LogStr.CRITICAL_WRONG_ARGUMENT, name, linkArea));
                    }
                }

                return true;
            }

            return false;
        }

        private IEnumerator JumpProcess(OffMeshLinkData offMeshLinkData)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = offMeshLinkData.endPos;

            Vector3 moveDirection = (endPosition - startPosition).normalized;
            moveDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            float height = 1.3f;
            float progress = 0;

            Vector3 midPosition = (startPosition + endPosition) / 2 + (Vector3.up * height);
            float curvedDistance = Vector3.Distance(startPosition, midPosition) + Vector3.Distance(midPosition, endPosition);
            float linkDuration = curvedDistance / JUMP_SPEED;

            float clipLength = _clipJump.length;
            float frameDuration = 1f / _clipJump.frameRate;
            float timeForStartJumpFrames = frameDuration * JUMP_START_FRAMES;
            float timeForJumpFrames = frameDuration * (JUMP_FINISH_FRAMES - JUMP_START_FRAMES);
            float timeForStopJumpFrames = clipLength - (frameDuration * JUMP_FINISH_FRAMES);

            float speedMultiplier = timeForJumpFrames / linkDuration;

            _animatorController.SetFloat(JumpAnimationSpeed, speedMultiplier);
            _animatorController.SetTrigger(JumpTrigger);

            float adjustedStartDuration = timeForStartJumpFrames / speedMultiplier;
            yield return new WaitForSeconds(adjustedStartDuration);

            bool isJumpSoundPlayed = false;
            float jumpSoundDelay = 0.05f;

            while (progress < linkDuration)
            {
                progress += Time.deltaTime;

                float t = Mathf.Clamp01(progress / linkDuration);
                Vector3 pos = GetBezierPoint(startPosition, midPosition, endPosition, t);
                _navMeshAgent.transform.position = pos;
                float step = _rotationSpeed * Time.deltaTime;

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);

                if (!isJumpSoundPlayed && t > jumpSoundDelay)
                {
                    _soundsManager.PlaySound(_jumpDone, volume: JUMP_VOLUME, is2D: true);
                    isJumpSoundPlayed = true;
                }

                yield return _cachedWaitWhilePause;
            }

            float adjustedEndDuration = timeForStopJumpFrames / speedMultiplier;
            yield return new WaitForSeconds(adjustedEndDuration);

            _navMeshAgent.CompleteOffMeshLink();
            _linkProcess = null;
        }

        private IEnumerator ClimbProcess(OffMeshLinkData offMeshLinkData)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = offMeshLinkData.endPos;

            if (startPosition.y < endPosition.y)
                OnUp?.Invoke();
            else
                OnDown?.Invoke();

            Vector3 moveDirection = (endPosition - startPosition).normalized;
            moveDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            float linkDuration = Vector3.Distance(startPosition, endPosition) / LINK_SPEED;
            float progress = 0;

            while (progress < linkDuration)
            {
                progress += Time.deltaTime;
                float step = _rotationSpeed * Time.deltaTime;

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
                _navMeshAgent.transform.position = Vector3.Lerp(startPosition, endPosition, progress / linkDuration);

                yield return _cachedWaitWhilePause;
            }

            _navMeshAgent.CompleteOffMeshLink();
            _linkProcess = null;
        }

        private Vector3 GetBezierPoint(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1 - t;
            return (u * u * a) + (2 * u * t * b) + (t * t * c);
        }

        private bool TryInteract()
        {
            if (IsAccessibleTarget())
            {
                _interactCoroutine = StartCoroutine(InteractRoutine(_interactable));
                _interactable = null;
                return true;
            }

            return false;
        }

        private IEnumerator InteractRoutine(IInteractable interactable)
        {
            if (interactable.NeedLookAt)
            {
                Vector3 targetDirection = interactable.TargetLookAt - transform.position;
                targetDirection.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                while (Quaternion.Angle(transform.rotation, targetRotation) > MAX_TARGET_ANGLE_OFFSET)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
                    yield return _cachedWaitWhilePause;
                }
            }

            interactable.Interact();
            IsAutopilot = false;
        }

        private bool IsAccessibleTarget()
        {
            if (_interactable == null)
            {
                return false;
            }

            Vector3 target = _interactable.TargetPosition;
            target.y = transform.position.y;
            return !_interactable.NeedComeUp || Vector3.Distance(transform.position, target) <=
                Math.Max(_interactable.MaxDistance, _navMeshAgent.stoppingDistance);
        }

        private void SetDestination(Vector3 destination, bool spawnEffect = true)
        {
            _destination = (_interactable != null && _interactable.NeedComeUp) ?
                    _interactable.TargetPosition : destination;

            if (spawnEffect)
            {
                _lastClickPosition = destination;
                Instantiate(_clickEffectSplash, destination, _clickEffectSplash.transform.rotation * Quaternion.Euler(new Vector3(0,0, UnityEngine.Random.Range(0,360))));
            }
        }

        private void SetInterableObject(IInteractable interactableObject)
        {
            if (_interactCoroutine != null)
            {
                StopCoroutine(_interactCoroutine);
                _interactCoroutine = null;
            }

            _interactable = interactableObject;
        }

        private void SetAutoTarget(IInteractable interactable)
        {
            OnClickRelease();
            IsAutopilot = true;
            SetInterableObject(interactable);
            SetDestination(interactable != null && interactable.NeedComeUp ? interactable.TargetPosition : transform.position);
            Move();
        }
    }
}