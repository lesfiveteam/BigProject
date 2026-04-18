using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace BigProject.Gameplay.Watermill
{
    public class ControlPanelStateIncompleted : IControlPanelState
    {
        private ControlPanel _controlPanel;
        private PlayerInputHandler _input;
        private GameObject _repairedLeverHolder;
        private GameObject _repairedLever;
        private IQuestActionHandler _installLeverAction;
        private InventorySystem _inventory;
        private CancellationTokenSource _crSource;
        private float _leverInstallTime;
        private bool _isSkipped = true;
        private UnityEvent _incompleteClue;
        private SoundsManager _soundsManager;
        private AudioClip _leverInsertSound;

        public ControlPanelStateIncompleted(ControlPanel controlPanel, PlayerInputHandler input, GameObject repairedLeverHolder, GameObject repairedLever,
            float leverInstallTime, IQuestActionHandler installLeverAction, InventorySystem inventory, UnityEvent incompleteClue,
            SoundsManager soundsManager, AudioClip leverInsertSound)
        {
            _controlPanel = controlPanel;
            _input = input;
            _repairedLeverHolder = repairedLeverHolder;
            _repairedLever = repairedLever;
            _leverInstallTime = leverInstallTime;
            _installLeverAction = installLeverAction;
            _installLeverAction.StateChanged += OnStateChanged;
            OnStateChanged();
            _inventory = inventory;
            _incompleteClue = incompleteClue;
            _soundsManager = soundsManager;
            _leverInsertSound = leverInsertSound;
        }

        public bool IsReady => _installLeverAction.CurrentState == QuestActionState.Active;

        public void Start()
        {
            _repairedLeverHolder.SetActive(true);
            OnStateChanged();
        }
        public void OnClicked()
        {
            if (GameplayUtilities.TryGetClickedObject(_input.GetMousePosition(), out GameObject go) &&
                go.TryGetComponent(out CapsuleCollider _) && _isSkipped)
            {
                _incompleteClue?.Invoke();
            }
        }

        // Apply repaired lever to panel.
        public void ApplyItem(Item item)
        {
            _crSource = new();

            try
            {
                _inventory.RemoveItemByName(item._name);
            }
            catch (Exception ex)
            {
                string msg = $"Some error ocurred while releasing the item from inventory: {ex.Message}";
                Managers.GameLogManager.Critical(msg);
                Debug.Log(msg);
            }

            _ = InstallLever(_crSource.Token);
        }

        public void Dispose()
        {
            if (_isSkipped)
            {
                // Set lever final position.
                _repairedLever.transform.localPosition = _repairedLeverHolder.transform.localPosition;
                SetVisibility();
            }

            _installLeverAction.StateChanged -= OnStateChanged;
            _crSource?.Cancel();
            _crSource?.Dispose();
        }

        private async Awaitable InstallLever(CancellationToken ct)
        {
            _soundsManager.PlaySound(_leverInsertSound, is2D: true);
            _isSkipped = false;
            SetVisibility();
            await _controlPanel.MoveLever(_repairedLever.transform, _repairedLeverHolder.transform.localPosition, _leverInstallTime, ct);
            MakeTransition();
        }

        private void MakeTransition()
        {
            try
            {
                _installLeverAction.MakeTransition(0);
            }
            catch (Exception ex)
            {
                string msg = $"Unable to make action transition from installing repaired lever: {ex.Message}";
                GameLogManager.Critical(msg);
                Debug.Log(msg);
            }
        }

        private void OnStateChanged()
        {
            if (_installLeverAction.CurrentState >= QuestActionState.Completed)
            {
                _controlPanel.ChangeState(ControlPanelState.Fixed);
            }
        }

        private void SetVisibility()
        {
            _repairedLever.SetActive(true);
            _repairedLeverHolder.SetActive(false);
        }
    }
}