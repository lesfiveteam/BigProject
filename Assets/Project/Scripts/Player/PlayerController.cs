using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Intercatable;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Systems.Sound;
using BigProject.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BigProject.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Animator _animatorController;

        [SerializeField] private float _navMeshHitPointDistance = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _climbSpeed = 0.2f;

        private PlayerInputHandler _inputHandler;
        private IInteractable _interactable = null;
        private Vector3 _destination;
        private bool _isMoving;

        private Coroutine _climbProcess;
        private float _climbDuration;

        private Camera _camera;
        private SceneLoadManager _sceneLoader;
        private SoundsManager _soundsManager;

        private const string MOVING_ANIM_BOOL = "IsMoving";

        public bool IsMoving => _isMoving;

        public void Init(PlayerInputHandler inputHandler, SceneLoadManager sceneLoader, SoundsManager soundsManager)
        {
            _inputHandler = inputHandler;
            _sceneLoader = sceneLoader;
            _soundsManager = soundsManager;
            ExceptionUtilities.ThrowIfNull(_inputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerInputHandler"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SoundsManager"));
            _navMeshAgent.updateRotation = false;
        }

        private void Start()
        {
            FindCamera();
        }

        private void OnEnable()
        {
            _inputHandler.Click += OnClick;
            _sceneLoader.SceneLoadingCompleted += OnSceneLoadingCompleted;
        }
        private void OnDisable()
        {
            _inputHandler.Click -= OnClick;
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
            if (GameplayUtilities.IsPointerOverUI())
            {
                return;
            }

            Vector2 mousePosition = _inputHandler.GetMousePosition();
            Ray ray = _camera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, _navMeshHitPointDistance, NavMesh.AllAreas))
                {
                    SetDestination(navMeshHit.position);
                    Move();

                    // handing interactable object over to player
                    IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();
                    SetInterableObject(interactableObject);
                }
            }
        }

        private void Update()
        {
            if (TryClimb())
                return;

            if (_isMoving)
            {
                RotateTowardsMovement();

                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                {
                    if (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        _isMoving = false;
                        _animatorController.SetBool(MOVING_ANIM_BOOL, false);
                        Interact();
                    }
                }
            }
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

        public void Move()
        {
            _isMoving = true;
            _animatorController.SetBool(MOVING_ANIM_BOOL, true);
            _navMeshAgent.SetDestination(_destination);
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
                    _soundsManager.PlaySound(groundSounds.GetStepSound(), spawnPosition:transform);
                }
            }
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
            Vector3 startPosition = offMeshLinkData.startPos;
            Vector3 endPosition = offMeshLinkData.endPos;

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

                yield return null;
            }

            _navMeshAgent.CompleteOffMeshLink();
            _climbProcess = null;
        }

        private void Interact()
        {
            if (_interactable != null)
                _interactable.Interact();
        }

        public void SetDestination(Vector3 destination)
        {
            _destination = destination;
        }

        public void SetInterableObject(IInteractable interactableObject)
        {
            _interactable = interactableObject;
        }
    }
}