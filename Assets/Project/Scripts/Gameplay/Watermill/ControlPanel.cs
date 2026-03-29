using BigProject.Gameplay.Common;
using BigProject.Intercatable;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Systems.Inventory;
using BigProject.Utilities;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.Watermill
{
    public enum ControlPanelState
    {
        Broken,
        Incompleted,
        Fixed,
        Completed
    }

    /// <summary>
    /// Levers Puzzle Panel
    /// </summary>
    public class ControlPanel : MonoBehaviour, IInteractable
    {
        [Header("Base settings")]
        [SerializeField]
        private MiniGameActivator _activator;
        [SerializeField]
        private GearsHandler _gearsHandler;
        [SerializeField]
        private QuestActionHandlersContainer _actions;
        [SerializeField]
        private int _noteItemId;

        [Header("Levers settings")]
        [SerializeField]
        private GameObject _brokenLever;
        [SerializeField]
        private GameObject _repairedLeverHolder;
        [SerializeField]
        private GameObject _repairedLever;
        [SerializeField]
        private float _brokenLeverOffset = 1f;
        [SerializeField]
        private float _brokenLeverRemoveTime = 2f;
        [SerializeField]
        private float _repairedLeverInstallTime = 1f;
        [SerializeField]
        private int _brokenLeverItemId;
        [SerializeField]
        private float _leverMoveTime = 1f;
        [SerializeField]
        private float _leverStaggerTime = 0.2f;
        [SerializeField]
        private float _staggerDistance = 0.1f;
        [SerializeField]
        private List<Lever> _levers;
        [SerializeField]
        private List<Transform> _leversPoints;
        [SerializeField]
        private AudioClip _fixedMillMusic;
        [SerializeField]
        private float _musicFadeInTime = 0.1f;
        [SerializeField]
        private float _musicFadeOutTime = 0.1f;

        private PlayerInputHandler _inputHandler;
        private InventorySystem _inventory;
        private IControlPanelState _state;
        private ControlPanelState _currentPanelState;
        private bool _isLeverMoving;
        private Vector2 _deltaInversion = new(-1f, 1f);
        private GameplayManager _gameplayManager;
        private MusicManager _musicManager;

        public ControlPanelState CurrentPanelState => _currentPanelState;

        public void Init(GameplayManager gameplayManager, PlayerInputHandler inputHandler, InventorySystem inventory, MusicManager musicManager)
        {
            _gameplayManager = gameplayManager;
            _inputHandler = inputHandler;
            _inventory = inventory;
            _musicManager = musicManager;
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Gameplay Manager"));
            ExceptionUtilities.ThrowIfNull(_inputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Input Handler"));
            ExceptionUtilities.ThrowIfNull(_inventory, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_musicManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Music Manager"));
            ChangeState(ControlPanelState.Broken);
        }

        public void Awake()
        {
            Assert.IsNotNull(_activator, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Activator"));
            Assert.IsNotNull(_actions, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Actions Container"));
            Assert.IsNotNull(_brokenLever, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Broken Lever"));
            Assert.IsNotNull(_repairedLeverHolder, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Repaired Lever Holder"));
            Assert.IsNotNull(_repairedLever, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Repaired Lever"));
            Assert.IsNotNull(_gearsHandler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Gears Handler"));
        }

        private void OnDestroy()
        {
            _state?.Dispose();
        }

        public void Interact()
        {
            if (_state != null && _state.IsReady)
            {
                _activator.ActivateMiniGame();
            }
        }

        public void Deactivate()
        {
            _activator.DeactivateMiniGame();
        }

        public void PlayFixedMusic()
        {
            _musicManager.PlayMusic(_fixedMillMusic, _musicFadeOutTime, _musicFadeInTime, true);
        }

        public async Awaitable MoveLever(Transform lever, Vector3 targetPosition, float time, CancellationToken ct)
        {
            if (_isLeverMoving)
            {
                return;
            }

            _isLeverMoving = true;
            lever.DOLocalMove(targetPosition, time);
            await Awaitable.WaitForSecondsAsync(time + 0.1f, cancellationToken: ct);
            _isLeverMoving = false;
        }

        private void OnActivated(bool isActivated)
        {
            if (isActivated)
            {
                _state?.Start();
            }
            else
            {
                _state?.Stop();
            }
        }

        private void OnClicked()
        {
            if (!_activator.IsActivated)
            {
                return;
            }

            _state?.OnClicked();
        }

        private void OnSwiped(Vector2 delta)
        {
            if (!_activator.IsActivated)
            {
                return;
            }

            _state?.OnSwiped(delta * _deltaInversion);
        }

        private void OnUnclicked()
        {
            if (!_activator.IsActivated)
            {
                return;
            }

            _state?.OnUnclicked();
        }

        public void ChangeState(ControlPanelState newState)
        {
            _state?.Dispose();
            _currentPanelState = newState;
            GameLogManager.Info($"Control panel change state to: {_currentPanelState}");

            switch (_currentPanelState)
            {
                case ControlPanelState.Broken:
                    _state = new ControlPanelStateBroken(this, _inputHandler, _brokenLever, 
                        _brokenLeverOffset, _brokenLeverRemoveTime, _brokenLeverItemId, _actions["GetBrokenLever"], _inventory);
                    break;
                case ControlPanelState.Incompleted:
                    _state = new ControlPanelStateIncompleted(this, _inputHandler, _repairedLeverHolder, 
                        _repairedLever, _repairedLeverInstallTime, _actions["InstallLever"], _inventory);
                    break;
                case ControlPanelState.Fixed:
                    _state = new ControlPanelStateFixed(this, _inputHandler, _leversPoints, _levers, _leverMoveTime,
                        _leverStaggerTime, _staggerDistance, _noteItemId, _gearsHandler, _actions["ActivateMech"], _inventory);

                    //if (_activator.IsActivated)
                    //{
                    //    _inventoryUI.SetNoteVisibility(true);
                    //}

                    break;
                default:
                    _state = null;
                    break;
            }

            _state?.Start();
        }

        public void ApplyItem(Item item)
        {
            if (_activator.IsActivated)
            {
                _state?.ApplyItem(item);
            }
        }

        private void OnEnable()
        {
            _activator.Activated += OnActivated;
            _inputHandler.MiniGameClick += OnClicked;
            _inputHandler.MiniGameSwipe += OnSwiped;
            _inputHandler.MiniGameUnclick += OnUnclicked;
        }

        private void OnDisable()
        {
            _activator.Activated -= OnActivated;
            _inputHandler.MiniGameClick -= OnClicked;
            _inputHandler.MiniGameSwipe -= OnSwiped;
            _inputHandler.MiniGameUnclick -= OnUnclicked;
        }
    }
}