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
        private const string BOY_NAME = "Эйрик";
        private const float ANIMATION_DURATION = 0.3f;
        private const string DIALOGUE_ANIM_TRIGGER = "Pressed";
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
        private float _speakerImageTone = 0.5f;

        [SerializeField]
        private List<Button> _answerOptionButtons = new List<Button>();

        private DialogueManager _dialogueManager;

        private List<TextMeshProUGUI> _answerOptionButtonTexts = new List<TextMeshProUGUI>();
        private TextMeshProUGUI _leftNameTMPro;
        private TextMeshProUGUI _rightNameTMPro;

        private bool _answerWasShownPreviousFrame = false;
        private bool _isFirstLine;
        private bool _isAnimating = false;
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

            _dialogueManager = dialogueManager;
            // Обработчик нажатия на кнопку "Продолжить"
            _nextButton.onClick.AddListener(() => StartCoroutine(WaitAnimationEndAndShowNextStep()));
        }

        private IEnumerator WaitAnimationEndAndShowNextStep()
        {
            if (!_isAnimating)
            {
                _isAnimating = true;
                _dialogueManager.ShowNextStep();
                yield return new WaitForSeconds(ANIMATION_DURATION);
                _isAnimating = false;
            }
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
            SetDarkenCharacter(_rightCharacterImage, _speakerImageTone);
            SetDarkenCharacter(_leftCharacterImage, 1f);
            // Включаем отображение кнопки продолжить и текст NPC
            _nextButton.gameObject.SetActive(false);
            _dialogueTextFront.gameObject.SetActive(false);
            _dialogueTextBack.gameObject.SetActive(false);

            // Saying Player (left character)
            _leftCharacterNameField.SetActive(true);
            _rightCharacterNameField.SetActive(false);

            // Change character sprites
            if (dialogueLine.StartLeftCharacterSprite)
            {
                _leftCharacterImage.sprite = dialogueLine.StartLeftCharacterSprite;
            }
            if (dialogueLine.StartRightCharacterSprite)
            {
                _rightCharacterImage.sprite = dialogueLine.StartRightCharacterSprite;
            }

            // Answer options only for Boy
            _leftNameTMPro.text = BOY_NAME;

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
            if (dialogueNPCPhrase.IsRightSpeaker)
            {
                // Saying NPC (right character)
                SetDarkenCharacter(_leftCharacterImage, _speakerImageTone);
                SetDarkenCharacter(_rightCharacterImage, 1f);
                _rightNameTMPro.text = dialogueNPCPhrase.Name;
            } 
            else
            {
                // Saying NPC (left character)
                SetDarkenCharacter(_rightCharacterImage, _speakerImageTone);
                SetDarkenCharacter(_leftCharacterImage, 1f);
                _leftNameTMPro.text = dialogueNPCPhrase.Name;
            }

            _rightCharacterImage.sprite = dialogueNPCPhrase.RightCharacterSprite;
            _leftCharacterImage.sprite = dialogueNPCPhrase.LeftCharacterSprite;

            _leftCharacterNameField.SetActive(!dialogueNPCPhrase.IsRightSpeaker);
            _rightCharacterNameField.SetActive(dialogueNPCPhrase.IsRightSpeaker);
            // Включаем возможность продолжить
            _nextButton.gameObject.SetActive(true);



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

        private void SetDarkenCharacter(Image characterImage, float tone)
        {
            Color color = new Color(tone, tone, tone);
            characterImage.color = color;
        }
    }
}
