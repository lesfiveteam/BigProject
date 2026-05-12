using BigProject.Managers;
using BigProject.Systems.DialogueSystem;
using BigProject.Systems.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        private const string BOY_NAME = "Эйрик";
        private const string DIALOGUE_ANIM_TRIGGER = "Pressed";
        private const string ITEM_ANIM_TRIGGER = "Appear";
        // Hack - determine that this is a boy sprite and reduce it if so
        private const string BASE_BOY_SPRITE_NAME = "эмоции_мальчик";
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
        private Animator _dialogueMaskBackAnimator;
        [SerializeField]
        private Animator _dialogueAnswersAnimator;
        [SerializeField]
        private Color _chosenAnwerColor = Color.grey;
        [SerializeField]
        private Color _notChosenAnwerColor = Color.black;
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
        private float _defaultRectTransformHeight = 1400;
        [SerializeField]
        private float _boyRectTransformHeight = 1240;
        [SerializeField]
        private Animator _itemBlobAnimator;
        [SerializeField]
        private TextMeshProUGUI _itemBlobText;
        [SerializeField]
        private Image _itemImage;
        [SerializeField]
        private Color _colorForRecievingItem;
        [SerializeField]
        private Color _colorForGivingItem;
        [SerializeField]
        private float _speakerImageTone = 0.5f;

        private List<string> _itemsToIgnoreWhenAdding = new List<string>{"church_note_1", "church_note_2", "church_note_3", "church_note_4" };  //needed for the third quest 

        [SerializeField]
        private List<Button> _answerOptionButtons = new List<Button>();

        private DialogueManager _dialogueManager;

        private List<TextMeshProUGUI> _answerOptionButtonTexts = new List<TextMeshProUGUI>();
        private TextMeshProUGUI _leftNameTMPro;
        private TextMeshProUGUI _rightNameTMPro;

        private RectTransform _rectTransform;

        private bool _answerWasShownPreviousFrame = false;
        private bool _isFirstLine;
        private bool _isAnimating = false;
        private InventorySystem _inventorySystem;

        private List<Item> _previouslyheldItems = new List<Item>();

        public void Init(DialogueManager dialogueManager, InventorySystem inventorySystem)
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

            _rectTransform = _leftCharacterImage.GetComponent<RectTransform>();

            // Name fields
            _leftNameTMPro = _leftCharacterNameField.GetComponentInChildren<TextMeshProUGUI>();
            _rightNameTMPro = _rightCharacterNameField.GetComponentInChildren<TextMeshProUGUI>();

            _dialogueManager = dialogueManager;
            // Обработчик нажатия на кнопку "Продолжить"
            _nextButton.onClick.AddListener(ShowNextStep);

            _inventorySystem = inventorySystem;
            _inventorySystem.OnInventoryUpdated += InventoryUpdated;
        }

        private void InventoryUpdated()
        {
            List<Item> heldItems = _inventorySystem.GetAllHeldItems();

            if (_dialogueWindow.gameObject.activeInHierarchy)
            {
                if (_previouslyheldItems.Count < heldItems.Count) //Added something
                {
                    Item addedItem = heldItems[heldItems.Count - 1];
                    if (!_itemsToIgnoreWhenAdding.Contains(addedItem._name))
                    {
                        _itemImage.sprite = addedItem._itemSprite;
                        _itemBlobText.text = $"{addedItem._verbToUseInDialogueText} <color=#{_colorForRecievingItem.ToHexString()}>{addedItem._nameLocalized}</color>";
                        _itemBlobAnimator.SetTrigger(ITEM_ANIM_TRIGGER);
                    }
                }
                else if (_previouslyheldItems.Count > heldItems.Count) //Removed something
                {
                    Item removedItem = _previouslyheldItems[_previouslyheldItems.Count - 1];
                    _itemImage.sprite = removedItem._itemSprite;
                    _itemBlobText.text = $"{removedItem._verbToUseInDialogueText} <color=#{_colorForGivingItem.ToHexString()}>{removedItem._nameLocalized}</color>";
                    _itemBlobAnimator.SetTrigger(ITEM_ANIM_TRIGGER);
                }
            }
            _previouslyheldItems = heldItems;        
        }

        private void ShowNextStep()
        {
            if (!_isAnimating)
            {
                _dialogueManager.ShowNextStep();
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
            _rightCharacterImage.enabled = true;
            _leftCharacterImage.enabled = true;

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
            ResizeBoyRectTtransform(_boyRectTransformHeight);

            // Количество кнопок, которые нужно показать
            int buttonCount = Mathf.Min(
                _answerOptionButtons.Count,
                dialogueLine.DialogueAnswerOptions.Count
                );

            for (int i = 0; i < buttonCount; i++)
            {
                DialogueAnswerOption answer = dialogueLine.DialogueAnswerOptions[i];
                _answerOptionButtons[i].gameObject.SetActive(true);
                _answerOptionButtonTexts[i].text = answer.Text;
                // Styling for already chosen option
                _answerOptionButtonTexts[i].color = answer.IsChosenByDefault || _dialogueManager.IsAnswerChosen(answer)
                    ? _chosenAnwerColor
                    : _notChosenAnwerColor;
            }

            if (!_isFirstLine)
            {
                _dialogueBackgroundAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER);
                _dialogueAnswersAnimator.SetTrigger(DIALOGUE_ANIM_TRIGGER + "Top");
            }
            _isFirstLine = false;
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

            // 2026-03-25 Саня попросил скрывать картинку целиком в случае, когда не проставляем спрайт в SO
            if (dialogueNPCPhrase.RightCharacterSprite)
            {
                // Show new sprite
                _rightCharacterImage.sprite = dialogueNPCPhrase.RightCharacterSprite;
                _rightCharacterImage.enabled = true;
            }
            else
            {
                _rightCharacterImage.enabled = false;
            }

            if (dialogueNPCPhrase.LeftCharacterSprite)
            {
                // Show new sprite
                _leftCharacterImage.sprite = dialogueNPCPhrase.LeftCharacterSprite;
                _leftCharacterImage.enabled = true;
                float rectTransformHeight = dialogueNPCPhrase.LeftCharacterSprite.name.Contains(BASE_BOY_SPRITE_NAME)
                    ? _boyRectTransformHeight
                    : _defaultRectTransformHeight;
                ResizeBoyRectTtransform(rectTransformHeight);
            }
            else
            {
                _leftCharacterImage.enabled = false;
            }

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
            _isAnimating = true;
            yield return new WaitForSeconds(_dialogueBackgroundAnimationClip.length);
            _dialogueTextFront.text = _dialogueTextBack.text;
            _dialogueTextFront.gameObject.SetActive(true);
            _dialogueTextBack.gameObject.SetActive(false);
            _isAnimating = false;
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

        private void ResizeBoyRectTtransform(float height)
        {
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
