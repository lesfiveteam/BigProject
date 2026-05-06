using BigProject.Managers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.UI;
using BigProject.Utilities;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.Common
{
    /// <summary>
    /// Enter and exit from mini game.
    /// </summary>
    public class MiniGameActivator : MonoBehaviour
    {
        [SerializeField]
        private CinemachineCamera _gameCamera;
        [SerializeField]
        private float _exitTime = 0.5f;
        [SerializeField]
        private Collider _gameCollider;
        [SerializeField]
        private float _playerDisableFactor = 0.85f;
        [field: SerializeField]
        public bool ActivateGameColliderOnDeactivate { get; set; } = true;

        private SkinnedMeshRenderer _playerRenderer;
        private Collider _playerCollider;
        private GameplayManager _gameplayManager;
        private InventoryUI _inventoryUI;
        private PlayerInputHandler _inputHandler;

        public bool IsActivated { get; private set; }
        public event Action<bool> Activated;

        public void Init(GameplayManager gameplayManager, PlayerInputHandler inputHandler, InventoryUI inventoryUI,
            Collider playerCollider, SkinnedMeshRenderer playerRenderer)
        {
            _gameplayManager = gameplayManager;
            _inventoryUI = inventoryUI;
            _inputHandler = inputHandler;
            _playerCollider = playerCollider;
            _playerRenderer = playerRenderer;
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Gameplay Manager"));
            ExceptionUtilities.ThrowIfNull(_inputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Input Handler"));
            ExceptionUtilities.ThrowIfNull(_inventoryUI, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory UI"));
            ExceptionUtilities.ThrowIfNull(_playerCollider, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Collider"));
            ExceptionUtilities.ThrowIfNull(_playerRenderer, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Renderer"));
        }

        private void Awake()
        {
            Assert.IsNotNull(_gameCollider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Collider"));
            Assert.IsNotNull(_gameCamera, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Cinemachine Camera"));
        }

        private void OnEnable()
        {
            _inputHandler.Cancel += OnCanceld;
        }

        private void OnDisable()
        {
            _inputHandler.Cancel -= OnCanceld;
        }

        public void ActivateMiniGame()
        {
            if (!IsActivated)
            {
                StopAllCoroutines();
                StartCoroutine(ActivateRoutine());
                Activated?.Invoke(true);
            }
        }

        public void DeactivateMiniGame()
        {
            if (IsActivated)
            {
                Activated?.Invoke(false);
                StopAllCoroutines();
                StartCoroutine(DeactivateRoutine());
            }
        }

        private IEnumerator ActivateRoutine()
        {
            IsActivated = true;
            _gameCamera.enabled = true;
            _gameplayManager.ChangeState(GameplayState.MiniGame);
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(GameplayUtilities.CurrentCameraTransitionTime * _playerDisableFactor);
            _playerRenderer.enabled = false;
            _playerCollider.enabled = false;
            yield return new WaitForSeconds(GameplayUtilities.CurrentCameraTransitionTime);
            _gameCollider.enabled = false;
            _inventoryUI.SetNoteVisibility(true);

        }

        private IEnumerator DeactivateRoutine()
        {
            IsActivated = false;
            _inventoryUI.SetNoteVisibility(false);
            yield return new WaitForSeconds(_exitTime);
            _gameCamera.enabled = false;
            yield return new WaitForFixedUpdate(); // Wait one frame for calculate camera transition.
            yield return new WaitForSeconds(GameplayUtilities.CurrentCameraTransitionTime * (1f - _playerDisableFactor));
            _playerRenderer.enabled = true;
            _playerCollider.enabled = true;
            yield return new WaitForSeconds(GameplayUtilities.CurrentCameraTransitionTime);
            _gameCollider.enabled = ActivateGameColliderOnDeactivate;
            _gameplayManager.ChangeState(GameplayState.Play);
        }

        private void OnCanceld()
        {
            DeactivateMiniGame();
        }
    }
}