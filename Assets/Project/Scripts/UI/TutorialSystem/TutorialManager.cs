using BigProject.Managers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.DialogueSystem;
using BigProject.Utilities;
using System;
using UnityEngine;

namespace BigProject.UI.TutorialSystem
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private DialogueLine _dialogueLineActivation;

        private DialogueManager _dialogueManager;
        private Tutorial _tutorial;
        private PlayerInputHandler _playerInputHandler;
        private GameplayManager _gameplayManager;
        private bool _isInitialized;

        public void Init(DialogueManager dialogueManager, Tutorial tutorial, PlayerInputHandler playerInputHandler, GameplayManager gameplayManager)
        {
            _dialogueManager = dialogueManager;
            _tutorial = tutorial;
            _playerInputHandler = playerInputHandler;
            _gameplayManager = gameplayManager;
            _dialogueManager.OnDialogueEnded += OnDialogueEnded;
            _playerInputHandler.ToggleTutorial += OnToggleTutorial;
            _gameplayManager.StateChanged += OnGameStateChanged;
            ExceptionUtilities.ThrowIfNull(_dialogueManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
            ExceptionUtilities.ThrowIfNull(_tutorial, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
            ExceptionUtilities.ThrowIfNull(_playerInputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
        }

        private void OnDialogueEnded(DialogueLine dialogueLine)
        {
            if (dialogueLine != _dialogueLineActivation)
            {
                return;
            }

            _gameplayManager.ChangeState(GameplayState.Tutorial);
        }

        private void OnToggleTutorial()
        {
            if (_tutorial.IsActive)
            {
                _gameplayManager.ChangeState(GameplayState.Play);
            }
            else if(_gameplayManager.State == GameplayState.Play)
            {
                _gameplayManager.ChangeState(GameplayState.Tutorial);
            }
        }

        private void OnGameStateChanged(GameplayState state)
        {
            if (_gameplayManager.State == GameplayState.Tutorial)
            {
                _tutorial.Activate(true);
            }
            else if (_tutorial.IsActive)
            {
                _tutorial.Activate(false);
            }
        }

        private void OnDestroy()
        {
            _dialogueManager.OnDialogueEnded -= OnDialogueEnded;
            _playerInputHandler.ToggleTutorial -= OnToggleTutorial;
            _gameplayManager.StateChanged -= OnGameStateChanged;
        }
    }
}