using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Intercatable;
using BigProject.Systems;
using BigProject.Utilities;
using System;
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

        private PlayerInputHandler _inputHandler;
        private IInteractable _interactable = null;
        private Vector3 _destination;
        private bool _isMoving;

        private Camera _camera;
        private SceneLoadManager _sceneLoader;

        private const string MOVING_ANIM_BOOL = "IsMoving";

        public bool IsMoving => _isMoving;

        public void Init(PlayerInputHandler inputHandler, SceneLoadManager sceneLoader)
        {
            _inputHandler = inputHandler;
            _sceneLoader = sceneLoader;
            ExceptionUtilities.ThrowIfNull(_inputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "PlayerInputHandler"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "SceneLoadManager"));
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

            if (Physics.Raycast(ray, out RaycastHit hit) 
                )
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
                moveDirection.y = 0; // Игнорируем вертикальную составляющую

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
