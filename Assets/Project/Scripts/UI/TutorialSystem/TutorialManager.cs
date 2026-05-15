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
            _isInitialized = true;
            ExceptionUtilities.ThrowIfNull(_dialogueManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
            ExceptionUtilities.ThrowIfNull(_tutorial, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
            ExceptionUtilities.ThrowIfNull(_playerInputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Tutorial manager"));
        }

        private void ActivateTutorial(bool isActive)
        {
            _gameplayManager.ChangeState(isActive ? GameplayState.Tutorial : GameplayState.Play);
            _tutorial.Activate(isActive);
        }

        private void OnDialogueEnded(DialogueLine dialogueLine)
        {
            if (dialogueLine != _dialogueLineActivation)
            {
                return;
            }

            ActivateTutorial(true);
        }

        private void OnToggleTutorial()
        {
            if (_tutorial.IsActive)
            {
                ActivateTutorial(false);
            }
            else if(_gameplayManager.State == GameplayState.Play)
            {
                ActivateTutorial(true);
            }
        }

        private void OnEnable()
        {
            if (_isInitialized)
            {
                _dialogueManager.OnDialogueEnded += OnDialogueEnded;
                _playerInputHandler.ToggleTutorial += OnToggleTutorial;
            }
        }

        private void OnDisable()
        {
            _dialogueManager.OnDialogueEnded -= OnDialogueEnded;
            _playerInputHandler.ToggleTutorial -= OnToggleTutorial;
        }
    }
}