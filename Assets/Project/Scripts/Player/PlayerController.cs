using Assets.Project.Scripts.Interactable;
using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Intercatable;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Systems.Sound;
using BigProject.Utilities;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace BigProject.Player
{
    public class PlayerController : MonoBehaviour
    {
        public Action OnUp;
        public Action OnDown;

        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Animator _animatorController;

        [SerializeField] private float _navMeshHitPointDistance = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _climbSpeed = 0.2f;
        [SerializeField] private bool _walkOnHolding = true;
        [SerializeField] private float _walkOnHoldingCheckInterval = 0.2f;

        private PlayerInputHandler _inputHandler;
        private IInteractable _interactable = null;
        private Vector3 _destination;
        private bool _isMoving;

        private Coroutine _climbProcess;
        private float _climbDuration;

        private Camera _camera;
        private SceneLoadManager _sceneLoader;
        private SoundsManager _soundsManager;
        private Coroutine _walkOnHoldingCoroutine;
        private WaitForSeconds _walkOnHolingCheckWait;
        private Coroutine _interactCoroutine;
        private float _animSpeed;
        private int _layerMask;

        private const string MOVING_ANIM_BOOL = "IsMoving";
        private const float MIN_WALK_SPEED_COEFF = 0.1f;
        private const float MIN_ANIM_SPEED_COEFF = 0.5f;
        private const float MAX_TARGET_ANGLE_OFFSET = 3f;

        public bool IsAutopilot { private set; get;}

        public void Init(PlayerInputHandler inputHandler, SceneLoadManager sceneLoader, SoundsManager soundsManager)
        {
            _inputHandler = inputHandler;
            _sceneLoader = sceneLoader;
            _soundsManager = soundsManager;
            ExceptionUtilities.ThrowIfNull(_inputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerInputHandler"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SoundsManager"));
            _navMeshAgent.updateRotation = false;
            _layerMask = ~((1 << LayerMask.NameToLayer("Scared")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));
            _animSpeed = _animatorController.speed;
            _walkOnHolingCheckWait = new(_walkOnHoldingCheckInterval);
        }

        public void AutoTarget(IInteractable interactable)
        {
            OnClickRelease();
            IsAutopilot = true;
            SetInterableObject(interactable);
            SetDestination(interactable != null && interactable.NeedComeUp ? interactable.TargetPosition : transform.position);
            Move();
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

                CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();

                if (brain == null)
                    Debug.LogError(string.Format(LogStr.ERROR_NULL_COMPONENT, typeof(CinemachineBrain)));

                ICinemachineCamera activeCamera = brain.ActiveVirtualCamera;

                if (activeCamera == null)
                    Debug.LogError(string.Format(LogStr.ERROR_NULL_COMPONENT, typeof(ICinemachineCamera)));

                CameraMove cameraMove = (activeCamera as CinemachineCamera).GetComponent<CameraMove>();

                if (cameraMove != null)
                    cameraMove.Subscribe(this);
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
            if (TryClimb())
                return;

            if (_isMoving)
            {
                RotateTowardsMovement();
                HandleWalkAnimation();

                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance &&
                    (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f))
                {                    
                    _isMoving = false;
                    _animatorController.SetBool(MOVING_ANIM_BOOL, false);
                    TryInteract();
                }
            }
        }

        private IEnumerator CalculateMovementRoutine()
        {
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

                        SetDestination(navMeshHit.position);
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
            _animatorController.SetBool(MOVING_ANIM_BOOL, isMovingAnim);

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

        private bool TryClimb()
        {
            if (_navMeshAgent.isOnOffMeshLink)
            {
                if (_climbProcess == null)
                {
                    _climbProcess = StartCoroutine(ClimbProcess(_navMeshAgent.currentOffMeshLinkData));
                }

                return true;
            }

            return false;
        }

        private IEnumerator ClimbProcess(OffMeshLinkData offMeshLinkData)
        {
            _isMoving = true;

            Vector3 startPosition = offMeshLinkData.startPos;
            Vector3 endPosition = offMeshLinkData.endPos;

            if (startPosition.y < endPosition.y)
                OnUp?.Invoke();
            else
                OnDown?.Invoke();

            Vector3 moveDirection = (endPosition - startPosition).normalized;
            moveDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            _climbDuration = Vector3.Distance(startPosition, endPosition) / _climbSpeed;

            float progress = 0;

            while (progress < _climbDuration)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );

                _navMeshAgent.transform.position = Vector3.Lerp(startPosition, endPosition, progress / _climbDuration);

                progress += Time.deltaTime;

                yield return new WaitWhile(() => Time.timeScale == 0);
            }

            _navMeshAgent.CompleteOffMeshLink();
            _climbProcess = null;
        }

        private bool TryInteract()
        {
            if (IsAccessibleTarget())
            {
                _interactCoroutine = StartCoroutine(InteractRoutine(_interactable));
                return true;
            }

            return false;
        }

        private IEnumerator InteractRoutine(IInteractable intercatable)
        {
            if (intercatable.NeedLookAt)
            {
                Vector3 targetDirection = intercatable.TargetLookAt - transform.position;
                targetDirection.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                while (Quaternion.Angle(transform.rotation, targetRotation) > MAX_TARGET_ANGLE_OFFSET)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
                    yield return null;
                }
            }

            intercatable.Interact();
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

        private void SetDestination(Vector3 destination)
        {
            _destination = (_interactable != null && _interactable.NeedComeUp) ?
                    _interactable.TargetPosition : destination;
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
    }
}