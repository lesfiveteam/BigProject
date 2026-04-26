using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using System.Threading;
using UnityEngine;

namespace BigProject.Gameplay.Watermill
{
    public class ControlPanelStateBroken : IControlPanelState
    {
        private ControlPanel _controlPanel;
        private PlayerInputHandler _input;
        private GameObject _brokenLever;
        private IQuestActionHandler _getBrokenLeverAction;
        private CancellationTokenSource _ctSource;
        private float _brokenLeverOffset;
        private float _brokenLeverRemoveTime;
        private int _brokenLeverItemId;
        private bool _isRemovingLever;
        private InventorySystem _inventory;
        private SoundsManager _soundsManager;
        private AudioClip _leverTakeSound;

        public ControlPanelStateBroken(ControlPanel controlPanel, PlayerInputHandler input, GameObject brokenLever, float brokenLeverOffset,
            float brokenLeverRemoveTime, int brokenLeverItemId, IQuestActionHandler getBrokenLeverAction, InventorySystem inventory, 
            SoundsManager soundsManager, AudioClip leverTakeSound)
        {
            _controlPanel = controlPanel;
            _input = input;
            _brokenLever = brokenLever;
            _brokenLeverOffset = brokenLeverOffset;
            _brokenLeverRemoveTime = brokenLeverRemoveTime;
            _brokenLeverItemId = brokenLeverItemId;
            _getBrokenLeverAction = getBrokenLeverAction;
            _getBrokenLeverAction.StateChanged += OnStateChanged;
            _isRemovingLever = false;
            _inventory = inventory;
            _soundsManager = soundsManager;
            _leverTakeSound = leverTakeSound;
        }

        public bool IsReady => _getBrokenLeverAction.CurrentState == QuestActionState.Active;

        public void Start() => OnStateChanged();

        public void OnClicked()
        {
            if (GameplayUtilities.TryGetClickedObject(_input.GetMousePosition(), out GameObject go) && go == _brokenLever && !_isRemovingLever)
            {
                _ctSource = new();
                _ = RemoveLever(_ctSource.Token);
            }
        }

        public void Dispose()
        {
            _brokenLever.SetActive(false);
            _getBrokenLeverAction.StateChanged -= OnStateChanged;
            _ctSource?.Cancel();
            _ctSource?.Dispose();
        }

        private async Awaitable RemoveLever(CancellationToken ct)
        {
            _soundsManager.PlaySound(_leverTakeSound, is2D: true);
            _isRemovingLever = true;
            Vector3 _targetPosition = _brokenLever.transform.localPosition;
            _targetPosition.z += _brokenLeverOffset;
            await _controlPanel.MoveLever(_brokenLever.transform, _targetPosition, _brokenLeverRemoveTime, ct);
            _brokenLever.SetActive(false);
            MakeTransition();
        }

        private void MakeTransition()
        {
            try
            {
                _inventory.AddItemByItemID(_brokenLeverItemId);
            }
            catch (Exception ex)
            {
                string msg = $"Some error ocurred adding the item to inventory: {ex.Message}";
                Debug.LogError(String.Format(LogStr.ERROR_QUEST, msg));
            }

            try
            {
                _getBrokenLeverAction.MakeTransition(0);
            }
            catch (Exception ex)
            {
                string msg = $"Unable to make action transition from broken lever: {ex.Message}";
                Debug.LogError(String.Format(LogStr.ERROR_QUEST, msg));
            }
        }

        private void OnStateChanged()
        {
            if (_getBrokenLeverAction.CurrentState >= QuestActionState.Completed)
            {
                _controlPanel.ChangeState(ControlPanelState.Incompleted);
                _controlPanel.Deactivate();
            }
        }
    }
}