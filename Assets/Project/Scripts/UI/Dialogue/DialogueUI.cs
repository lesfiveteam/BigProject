using BigProject.Managers;
using BigProject.Systems.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField]
        public GameObject _dialogueWindow;
        [SerializeField]
        public Animator _dialogueBackgroundAnimator;
        [SerializeField]
        public AnimationClip _dialogueBackgroundAnimationClip;
        [SerializeField]
        private TextMeshProUGUI _dialogueTextFront;
        [SerializeField]
        public Animator _dialogueMaskFrontAnimator;
        [SerializeField]
        private TextMeshProUGUI _dialogueTextBack;
        [SerializeField]
        public Animator _dialogueMaskBackAnimator;
        [SerializeField]
        public Animator _dialogueAnswersAnimator;
        [SerializeField]
        private Image _rightCharacterImage;
        [SerializeField]
        private Image _leftCharacterImage;
        [SerializeField]
        private Button _nextButton;
        [SerializeField]
        private GameObject _leftCharacterNameField;
        [SerializeField]
        private GameObject _rightCharacterNameField;

        [SerializeField]
        private float _speakerImageAlpha = 0.8f;
        [SerializeField]
        private float _speakerImageTone = 0.5f;

        [SerializeField]
        private List<Button> _answerOptionButtons = new List<Button>();

        private List<TextMeshProUGUI> _answerOptionButtonTexts = new List<TextMeshProUGUI>();
        private TextMeshProUGUI _leftNameTMPro;
        private TextMeshProUGUI _rightNameTMPro;

        private string DIALOGUE_ANIM_TRIGGER = "Pressed";
        private bool _answerWasShownPreviousFrame = false;
        private bool _isFirstLine;
        public void Init(DialogueManager dialogueManager)
        {
            for (int i = 0; i < _answerOptionButtons.Count; i++)
            {
                // Для замыкания
                int index = i;
                // Обработчик нажатия на вариант ответа
                _answerOptionButtons[i].onClick.AddListener(() => dialogueManager.SelectAnswerOption(index));
                // Инициализируем кнопки для взаимодействия
                TextMeshProUGUI buttonText = _answerOptionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText)
                {
                    _answerOptionButtonTexts.Add(buttonText);
                }
            }

            // Name fields
            _leftNameTMPro = _leftCharacterNameField.GetComponentInChildren<TextMeshProUGUI>();
            _rightNameTMPro = _rightCharacterNameField.GetComponentInChildren<TextMeshProUGUI>();

            // Обработчик нажатия на кнопку "Продолжить"
            _nextButton.onClick.AddListener(() => dialogueManager.ShowNextStep());
        }

        public void HideAnswerOptions()
        {
            _dialogueBackgroundAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER);
            _dialogueAnswersAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER + "Bottom");
            _answerWasShownPreviousFrame = true;
            StartCoroutine(WaitForAnimationFinishedAnswers());
        }

        public void ShowAnswerOptions(DialogueLine dialogueLine)
        {
            SetImageAlpha(_rightCharacterImage, _speakerImageTone, _speakerImageAlpha);
            SetImageAlpha(_leftCharacterImage, 1f, 1f);
            // Включаем отображение кнопки продолжить и текст NPC
            _nextButton.gameObject.SetActive(false);
            _dialogueTextFront.gameObject.SetActive(false);
            _dialogueTextBack.gameObject.SetActive(false);

            // Saying Player (left character)
            _leftCharacterNameField.SetActive(true);
            _rightCharacterNameField.SetActive(false);

            // Количество кнопок, которые нужно показать
            int buttonCount = Mathf.Min(
                _answerOptionButtons.Count,
                dialogueLine.DialogueAnswerOptions.Count
                );

            for (int i = 0; i < buttonCount; i++)
            {
                _answerOptionButtons[i].gameObject.SetActive(true);
                _answerOptionButtonTexts[i].text = dialogueLine.DialogueAnswerOptions[i].Text;
            }

            _dialogueBackgroundAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER);
            _dialogueAnswersAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER + "Top");
        }

        public void ShowDialogueWindow()
        {
            _isFirstLine = true;
            _dialogueTextFront.gameObject.SetActive(true);
            _dialogueWindow.SetActive(true);
        }
        public void HideDialogueWindow()
        {
            _dialogueWindow.SetActive(false);
        }
        public void ShowNPCPhrase(DialogueNPCPhrase dialogueNPCPhrase)
        {
            SetImageAlpha(_leftCharacterImage, _speakerImageTone, _speakerImageAlpha);
            SetImageAlpha(_rightCharacterImage, 1f, 1f);
            // Saying NPC (right character)
            _leftCharacterNameField.SetActive(false);
            _rightCharacterNameField.SetActive(true);
            _rightNameTMPro.text = dialogueNPCPhrase.Name;
            // Включаем отображение кнопки продолжить и текст NPC
            _nextButton.gameObject.SetActive(true);
            _rightCharacterImage.sprite = dialogueNPCPhrase.CharacterSprite;

            if (_isFirstLine)
            {
                _isFirstLine = false;
                _dialogueTextFront.text = dialogueNPCPhrase.Text;
                return;
            }

            //Playing animations
            _dialogueMaskBackAnimator.gameObject.SetActive(true);
            _dialogueTextBack.gameObject.SetActive(true);
            _dialogueTextBack.text = dialogueNPCPhrase.Text;
            _dialogueBackgroundAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER);
            _dialogueMaskBackAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER);

            if (!_answerWasShownPreviousFrame)
            {
                _dialogueTextFront.gameObject.SetActive(true);
                _dialogueMaskFrontAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER);
            }

            StartCoroutine(WaitForAnimationFinished());
        }

        private IEnumerator WaitForAnimationFinished()
        {
            yield return new WaitForSeconds(_dialogueBackgroundAnimationClip.length);
            _dialogueTextFront.text = _dialogueTextBack.text;
            _dialogueTextFront.gameObject.SetActive(true);
            _dialogueTextBack.gameObject.SetActive(false);
        }

        private IEnumerator WaitForAnimationFinishedAnswers()
        {
            yield return new WaitForSeconds(_dialogueBackgroundAnimationClip.length);
            foreach (Button answerOptionButton in _answerOptionButtons)
            {
                answerOptionButton.gameObject.SetActive(false);
            }
            _answerWasShownPreviousFrame = false;
        }

        private void SetImageAlpha(Image image, float tone, float alpha)
        {
            Color color = new(tone, tone, tone, alpha);
            image.color = color;
        }
    }
}
