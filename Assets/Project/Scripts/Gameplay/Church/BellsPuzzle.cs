using BigProject.Gameplay.Common;
using BigProject.Intercatable;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace BigProject.Gameplay.Church
{
    public class BellsPuzzle : MonoBehaviour, IInteractable
    {
        private const int ACTION_ID = 7;
        private const int QUEST_ID = 3;

        [SerializeField]
        private int _swipeDownValue = -10;
        [SerializeField]
        private List<Bell> _bells = new List<Bell>();
        [SerializeField]
        private List<int> _targetBellOrder = new List<int>();

        private List<int> _playerBellOrder = new List<int>();
        private MiniGameActivator _activator;
        private PlayerInputHandler _inputHandler;
        private Bell _clickedBell;

        private SoundsManager _soundsManager;
        private IQuestActionHandler _actionHandler;
        private ProgressManager _progressManager;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _wrongFirstBellRemark;
        [SerializeField]
        private LocalizedString _wrongSecondBellRemark;
        [SerializeField]
        private LocalizedString _isValidOrderRemark;
        [SerializeField]
        private LocalizedString _wrongFourthBellRemark;

        public void Init(
            PlayerInputHandler inputHandler,
            MiniGameActivator miniGameActivator,
            SoundsManager soundsManager,
            ProgressManager progressManager
            )
        {
            _activator = miniGameActivator;
            _inputHandler = inputHandler;
            _progressManager = progressManager;

            foreach (Bell bell in _bells)
            {
                bell.Init(soundsManager);
            }

            if (!_progressManager.TryGetQuestActionHandler(QUEST_ID, ACTION_ID, out _actionHandler))
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_QUEST, $"{gameObject.name} unable to get quest {QUEST_ID} action handler {ACTION_ID}"));
            }
        }

        public void Interact()
        {
            if (!_activator.IsActivated)
            {
                _inputHandler.MiniGameClick += OnClicked;
                _inputHandler.MiniGameSwipe += OnSwiped;
                _activator.Activated += OnActivatedMiniGame;
            }
            _activator.ActivateMiniGame();
        }

        private void OnActivatedMiniGame(bool activated)
        {
            if (!activated)
            {
                // Clear player attempts
                _playerBellOrder.Clear();
                ResetActions();
            }
        }

        private void OnClicked()
        {
            _clickedBell = null;
            if (GameplayUtilities.TryGetClickedObject(_inputHandler.GetMousePosition(), out GameObject go))
            {
                _clickedBell = go.GetComponent<Bell>();
            }
        }

        private void OnSwiped(Vector2 delta)
        {
            if (_clickedBell != null && delta.y < _swipeDownValue)
            {
                // Jingle bells! - ringing bellg
                _clickedBell.Ring();

                if (_actionHandler.CurrentState != QuestActionState.Active)
                {
                    _clickedBell = null;
                    return;
                }

                _playerBellOrder.Add(_clickedBell.Id);

                if (_playerBellOrder.Count > _targetBellOrder.Count)
                {
                    // take the last played bells - delete first clicked bell in player order
                    _playerBellOrder.RemoveAt(0);
                }

                if (_playerBellOrder.Count == _targetBellOrder.Count && BellsOrderIsRight())
                {
                   WinMiniGame();
                }

                _clickedBell = null;
            }
        }

        // The order of the bells is correct
        private bool BellsOrderIsRight()
        {
            bool result = true;
            // Какой-то из колольчиков неправильный, но не учитываем второй в порядке
            bool isWrongNotSecondBell = false;

            for (int i = 0; i < _targetBellOrder.Count; i++)
            {
                if (_targetBellOrder[i] != _playerBellOrder[i])
                {
                    // Find error in order
                    result = false;
                    if (i != 1)
                    {
                        isWrongNotSecondBell = true;
                    }
                }
            }

            if (!result && !isWrongNotSecondBell)
            {
                // Какой-то из колокольчиков неправильный и это именно второй в порядке
                ShowWrongBellReplica(_playerBellOrder[1]);
            }

            return result;
        }

        private void ShowWrongBellReplica(int bellId)
        {
            switch (bellId) 
            {
                case 1:
                    ReplicaManager.ShowReplica(_wrongFirstBellRemark);
                    break;
                case 2:
                    ReplicaManager.ShowReplica(_wrongSecondBellRemark);
                    break;
                case 4:
                    ReplicaManager.ShowReplica(_wrongFourthBellRemark);
                    break;
            }
        }

        private void WinMiniGame()
        {
            _actionHandler.MakeTransition(0);
            _activator.DeactivateMiniGame();
            ResetActions();
            ReplicaManager.ShowReplica(_isValidOrderRemark);
        }

        private void ResetActions()
        {
            _inputHandler.MiniGameClick -= OnClicked;
            _inputHandler.MiniGameSwipe -= OnSwiped;
            _activator.Activated -= OnActivatedMiniGame;
        }
    }
}
