using BigProject.Systems.DialogueSystem;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance;

        // Событие срабатывает во время фразы NPC, если указан Id
        public static event Action<int> OnDialoguePhrase;

        [SerializeField]
        private GameObject _dialogueWindow;
        [SerializeField]
        private TextMeshProUGUI _dialogueText;
        [SerializeField]
        private Image _rightCharacterImage;
        [SerializeField]
        private Button _nextButton;

        [SerializeField]
        private List<Button> _answerOptionButtons = new List<Button>();
        private List<TextMeshProUGUI> _answerOptionButtonTexts = new List<TextMeshProUGUI>();

        private DialogueLine _currentDialogueLine;
        private int _currentDialoguePhraseIndex = 0;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            foreach (var answerOptionButton in _answerOptionButtons)
            {
                // Инициализируем кнопки для взаимодействия
                TextMeshProUGUI buttonText = answerOptionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText)
                {
                    _answerOptionButtonTexts.Add(buttonText);
                }
            }
        }

        public void StartDialogue(DialogueLine dialogueLine)
        {
            if (dialogueLine == null) {
                Debug.LogWarning("Не проинициализировали диалог");
                return;
            }

            if (dialogueLine.DialogueNPCPhrases.Count == 0 && dialogueLine.DialogueAnswerOptions.Count == 0)
            {
                Debug.LogWarning("Не проинициализировали диалог");
                return;
            }

            _dialogueWindow.SetActive(true);
            _currentDialogueLine = dialogueLine;
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
                ShowAnswerOptions();
            }
            else
            {
                // Диалог окончен
                EndDialogue();
            }
        }

        private void ShowNextPhrase()
        {
            // Включаем отображение кнопки продолжить и текст NPC
            _nextButton.gameObject.SetActive(true);
            _dialogueText.gameObject.SetActive(true);

            DialogueNPCPhrase dialogueNPCPhrase =
                _currentDialogueLine.DialogueNPCPhrases[_currentDialoguePhraseIndex++];
            _dialogueText.text = dialogueNPCPhrase.Text;
            _rightCharacterImage.sprite = dialogueNPCPhrase.CharacterSprite;

            if (dialogueNPCPhrase.Id > 0)
            {
                // Есть идентификатор фразы - уведомляем о том, что сейчас была сказана эта фраза
                OnDialoguePhrase.Invoke(dialogueNPCPhrase.Id);
            }
        }

        public void SelectAnswerOption(int answerOptionIndex)
        {
            _currentDialogueLine =
                _currentDialogueLine.DialogueAnswerOptions[answerOptionIndex].DialogueLine;
            _currentDialoguePhraseIndex = 0;
            HideAnswerOptions();
            ShowNextStep();
        }

        private void EndDialogue()
        {
            HideAnswerOptions();
            _dialogueWindow.SetActive(false);
            _currentDialogueLine = null;
            _currentDialoguePhraseIndex = 0;
        }

        private void HideAnswerOptions()
        {
            foreach (var answerOptionButton in _answerOptionButtons)
            {
                answerOptionButton.gameObject.SetActive(false);
            }
        }

        private void ShowAnswerOptions()
        {
            // Включаем отображение кнопки продолжить и текст NPC
            _nextButton.gameObject.SetActive(false);
            _dialogueText.gameObject.SetActive(false);
            // Количество кнопок, которые нужно показать
            int buttonCount = Mathf.Min(
                _answerOptionButtons.Count, 
                _currentDialogueLine.DialogueAnswerOptions.Count
                );

            for (int i = 0; i < buttonCount; i++)
            {
                _answerOptionButtons[i].gameObject.SetActive(true);
                _answerOptionButtonTexts[i].text = _currentDialogueLine.DialogueAnswerOptions[i].Text;
            }
        }
    }
}
