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
        private List<Bell> _bellQuestOrder = new List<Bell>();

        private List<Bell> _bellPlayerOrder = new List<Bell>();
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

            foreach (Bell bell in _bellQuestOrder)
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
                _bellPlayerOrder.Clear();
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
                _bellPlayerOrder.Add(_clickedBell);

                if (_bellPlayerOrder.Count > _bellQuestOrder.Count)
                {
                    // take the last played bells - delete first clicked bell in player order
                    _bellPlayerOrder.RemoveAt(0);
                }

                if (_bellPlayerOrder.Count == _bellQuestOrder.Count && BellsOrderIsRight())
                {
                   WinMiniGame();
                }

                _clickedBell = null;
            }
        }

        // The order of the bells is correct
        private bool BellsOrderIsRight()
        {
            for (int i = 0; i < _bellQuestOrder.Count; i++)
            {
                if (_bellQuestOrder[i] != _bellPlayerOrder[i])
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
