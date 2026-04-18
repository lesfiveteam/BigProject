using BigProject.Gameplay.Common;
using BigProject.Intercatable;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Gameplay.Church
{
    public class BellsPuzzle : MonoBehaviour, IInteractable
    {
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

        public void Init(
            PlayerInputHandler inputHandler,
            MiniGameActivator miniGameActivator,
            SoundsManager soundsManager
            )
        {
            _activator = miniGameActivator;
            _inputHandler = inputHandler;

            foreach (Bell bell in _bells)
            {
                bell.Init(soundsManager);
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
            for (int i = 0; i < _targetBellOrder.Count; i++)
            {
                if (_targetBellOrder[i] != _playerBellOrder[i])
                {
                    // Find error in order
                    return false;
                }
            }
            return true;
        }

        private void WinMiniGame()
        {
            // todo - add quest action
            _activator.DeactivateMiniGame();
            ResetActions();
        }

        private void ResetActions()
        {
            _inputHandler.MiniGameClick -= OnClicked;
            _inputHandler.MiniGameSwipe -= OnSwiped;
            _activator.Activated -= OnActivatedMiniGame;
        }
    }
}
