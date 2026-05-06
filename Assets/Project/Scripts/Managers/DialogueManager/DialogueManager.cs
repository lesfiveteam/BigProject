using BigProject.Systems;
using BigProject.Systems.DialogueSystem;
using BigProject.Systems.Inventory;
using BigProject.UI.Dialogue;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Managers
{
    public class DialogueManager
    {
        // Событие срабатывает во время фразы NPC, если указан Id
        public static event Action<int> OnDialoguePhrase;
        public bool IsDialogue => _currentDialogueLine != null;


        private DialogueLine _currentDialogueLine;
        private int _currentDialoguePhraseIndex = 0;

        private DialogueUI _dialogueView;

        private HashSet<string> _chosenAnswers = new HashSet<string>();
        private GameplayManager _gameplayManager;

        public DialogueManager(DialogueUI dialogueView, GameplayManager gameplayManager)
        {
            _dialogueView = dialogueView;
            _gameplayManager = gameplayManager;
        }
        
        public void Init(InventorySystem inventorySystem)
        {
            _dialogueView.Init(this, inventorySystem);
            _dialogueView.HideDialogueWindow();
        }

        public void StartDialogue(DialogueLine dialogueLine)
        {
            if (!IsSuitableState())
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, "DialogueManager", "unsuitable state for dialogue"));
                return;
            }

            if (dialogueLine == null)
            {
                Debug.LogWarning("Не проинициализировали диалог");
                return;
            }

            if (dialogueLine.DialogueNPCPhrases.Count == 0 && dialogueLine.DialogueAnswerOptions.Count == 0)
            {
                Debug.LogWarning("Не проинициализировали диалог");
                return;
            }

            _gameplayManager.ChangeState(GameplayState.Dialogue);
            ReplicaManager.HideReplica();

            _currentDialogueLine = dialogueLine;
            _dialogueView.ShowDialogueWindow();
            ShowNextStep();
        }

        public void ShowNextStep()
        {
            if (!_currentDialogueLine)
            {
                // Нет продолжения диалога
                EndDialogue();
                return;
            }

            if (_currentDialogueLine.DialogueNPCPhrases.Count > _currentDialoguePhraseIndex)
            {
                // NPC ещё не договорил - показываем следующую фразу
                ShowNextPhrase();
            }
            else if (_currentDialogueLine.DialogueAnswerOptions.Count > 0)
            {
                // NPC договорил и игроку есть что сказать - отображаем варианты ответов
                _dialogueView.ShowAnswerOptions(_currentDialogueLine);
            }
            else
            {
                // Диалог окончен
                EndDialogue();
            }
        }

        private bool IsSuitableState() => _gameplayManager.State == GameplayState.Play ||
                _gameplayManager.State == GameplayState.Cutscene;

        private void ShowNextPhrase()
        {
            // Включаем отображение кнопки продолжить и текст NPC
            DialogueNPCPhrase dialogueNPCPhrase =
                _currentDialogueLine.DialogueNPCPhrases[_currentDialoguePhraseIndex];

            _dialogueView.ShowNPCPhrase(dialogueNPCPhrase);
            _currentDialoguePhraseIndex++;

            if (dialogueNPCPhrase.Id > 0)
            {
                // Есть идентификатор фразы - уведомляем о том, что сейчас была сказана эта фраза
                OnDialoguePhrase.Invoke(dialogueNPCPhrase.Id);
            }
        }

        public void SelectAnswerOption(int answerOptionIndex)
        {
            DialogueAnswerOption selectedAnwser = _currentDialogueLine.DialogueAnswerOptions[answerOptionIndex];
            MarkAnswerOptionAsChosen(selectedAnwser);

            _currentDialogueLine =
                _currentDialogueLine.DialogueAnswerOptions[answerOptionIndex].DialogueLine;
            _currentDialoguePhraseIndex = 0;
            _dialogueView.HideAnswerOptions();
            ShowNextStep();
        }

        private void EndDialogue()
        {
            _dialogueView.HideAnswerOptions();
            _dialogueView.HideDialogueWindow();
            _currentDialogueLine = null;
            _currentDialoguePhraseIndex = 0;

            if (ServiceLocator.TryGetService(out GameplayManager gameplayManager))
            {
                gameplayManager.ChangeState(GameplayState.Play);
            }
        }

        // return true, if already was chosen - exist in hashSet
        public bool IsAnswerChosen(DialogueAnswerOption answer)
        {
            return _chosenAnswers.Contains(answer.HashId);
        }

        private void MarkAnswerOptionAsChosen(DialogueAnswerOption answer)
        {
            _chosenAnswers.Add(answer.HashId);
        }
    }
}
