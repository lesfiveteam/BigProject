using UnityEngine;
using BigProject.Systems.HUD;
using System;
using TMPro;
using UnityEngine.Assertions;
using System.Collections;
using UnityEngine.UI;

namespace BigProject.UI
{
    /// <summary>
    /// Quest journal view.
    /// </summary>
    public class JournalUI : MonoBehaviour, IHUDWidget, IDisposable
    {
        [SerializeField]
        private GameObject _journalObj;
        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _task;
        [SerializeField]
        private Image _headerImage;
        [SerializeField]
        private Animator _checkmarkAnimator;
        [SerializeField] 
        private float _characterTypingTime = 1f;
        [SerializeField] 
        private float _shineTime = 0.3f;

        private Coroutine _taskAnimationCoroutine;
        private Coroutine _questAnimationCoroutine;

        private QuestJournal _journal;

        private string CHECKMARK_APPEAR_ANIM_TRIGGER = "Appear";
        private string CHECKMARK_DISAPPEAR_ANIM_TRIGGER = "Disappear";

        private string _awaitableQuest;
        private string _awaitableTask;

        public void Init(QuestJournal journal)
        {
            Assert.IsNotNull(journal, "Journal view unable to work with null journal.");
            _journal = journal;
            journal.QuestChanged += OnQuestStateChanged;
            journal.TaskChanged += OnTaskChanged;
        }

        public void Hide()
        {
            _journalObj.SetActive(false);
        }

        public void Show()
        {
            _journalObj.SetActive(true);

            if (!string.IsNullOrEmpty(_awaitableQuest))
            {
                OnQuestStateChanged(_awaitableQuest);
                _awaitableQuest = null;
            }

            if (!string.IsNullOrEmpty(_awaitableTask))
            {
                OnTaskChanged(_awaitableTask);
                _awaitableTask = null;
            }
        }

        public void OnQuestStateChanged(string name)
        {
            if (!_journalObj.activeSelf)
            {
                _awaitableQuest = name;
                return;
            }

            if (_questAnimationCoroutine != null)
            {
                StopCoroutine(_questAnimationCoroutine);
                _questAnimationCoroutine = null;
            }

            _questAnimationCoroutine = StartCoroutine(PlayQuestAnimations(name));
        }

        public void OnTaskChanged(string task)
        {
            if (!_journalObj.activeSelf)
            {
                _awaitableTask = task;
                return;
            }

            if (_taskAnimationCoroutine != null)
            {
                StopCoroutine(_taskAnimationCoroutine);
                _taskAnimationCoroutine = null;
            }

            _taskAnimationCoroutine = StartCoroutine(PlayTaskAnimations(task));
        }

        public void Dispose()
        {
            _journal.QuestChanged -= OnQuestStateChanged;
            _journal.TaskChanged -= OnTaskChanged;
        }
        private IEnumerator PlayHeaderShineAnimation()
        {
            _headerImage.material.SetInt("_ShouldPlay", 1);
            yield return new WaitForSeconds(_shineTime);
            _headerImage.material.SetInt("_ShouldPlay", 0);
        }
        private IEnumerator PlayTaskAnimations(string newText)
        {
            yield return new WaitForSeconds(_shineTime);
            yield return PlayTaskCrossOutAnimation();
            yield return PlayTaskEraseAnimation();
            yield return PlayTaskTypeAnimation(newText);
        }

        private IEnumerator PlayQuestAnimations(string newText)
        { 
            yield return PlayHeaderShineAnimation();
            yield return PlayQuestCrossOutAnimation();
            yield return PlayQuestEraseAnimation();
            yield return PlayQuestTypeAnimation(newText);
        }

        private IEnumerator PlayTaskCrossOutAnimation()
        {
            string initialText = _task.text.ToString();

            if (string.IsNullOrEmpty(initialText))
            {
                yield break;
            }

            _checkmarkAnimator.SetTrigger(CHECKMARK_APPEAR_ANIM_TRIGGER);
            WaitForSeconds delay = new WaitForSeconds(_characterTypingTime / initialText.Length);

            int currentCharacterIndex = 0;
            int maxLength = _task.text.Length;

            while (currentCharacterIndex < maxLength + 1)
            {
                string crossedOutText = "<s>" + initialText[..currentCharacterIndex] + "</s>" + initialText[currentCharacterIndex..];
                _task.text = crossedOutText;
                currentCharacterIndex++;

                yield return delay;
            }
        }

        private IEnumerator PlayQuestCrossOutAnimation()
        {
            string initialText = _name.text.ToString();

            if (string.IsNullOrEmpty(initialText))
            {
                yield break;
            }

            WaitForSeconds delay = new WaitForSeconds(_characterTypingTime / initialText.Length);

            int currentCharacterIndex = 0;
            int maxLength = _name.text.Length;

            while (currentCharacterIndex < maxLength + 1)
            {
                string crossedOutText = "<s>" + initialText[..currentCharacterIndex] + "</s>" + initialText[currentCharacterIndex..];
                _name.text = crossedOutText;
                currentCharacterIndex++;

                yield return delay;
            }
        }
        private IEnumerator PlayTaskEraseAnimation()
        {
            if (string.IsNullOrEmpty(_task.text))
            {
                yield break;
            }

            WaitForSeconds delay = new WaitForSeconds(_characterTypingTime / _task.text.Length);

            _task.maxVisibleCharacters = _task.text.Length;
            _task.text = _task.text.Substring(3, _task.text.Length - 7);
            _task.fontStyle = FontStyles.Strikethrough;

            while (_task.maxVisibleCharacters > 0)
            {
                _task.maxVisibleCharacters--;
                yield return delay;
            }
        }

        private IEnumerator PlayQuestEraseAnimation()
        {
            if (string.IsNullOrEmpty(_name.text))
            {
                yield break;
            }

            WaitForSeconds delay = new WaitForSeconds(_characterTypingTime / _name.text.Length);
            _name.maxVisibleCharacters = _name.text.Length;
            _name.text = _name.text.Substring(3, _name.text.Length - 7);
            _name.fontStyle = FontStyles.Strikethrough;

            while (_name.maxVisibleCharacters > 0)
            {
                _name.maxVisibleCharacters--;
                yield return delay;
            }
        }

        private IEnumerator PlayTaskTypeAnimation(string newText)
        {
            _task.fontStyle = FontStyles.Normal;
            _checkmarkAnimator.SetTrigger(CHECKMARK_DISAPPEAR_ANIM_TRIGGER);
            _task.text = newText;
            _task.maxVisibleCharacters = 0;

            WaitForSeconds delay = new WaitForSeconds(_characterTypingTime / newText.Length);

            int currentVisibleCharacterIndex = 0;

            while (currentVisibleCharacterIndex < _task.text.Length + 1)
            {
                _task.maxVisibleCharacters++;
                yield return delay;

                currentVisibleCharacterIndex++;
            }
        }

        private IEnumerator PlayQuestTypeAnimation(string newText)
        {
            _name.fontStyle = FontStyles.Normal;
            _name.text = newText;
            _name.maxVisibleCharacters = 0;

            WaitForSeconds delay = new WaitForSeconds(_characterTypingTime / newText.Length);

            int currentVisibleCharacterIndex = 0;

            while (currentVisibleCharacterIndex < _name.text.Length + 1)
            {
                _name.maxVisibleCharacters++;
                yield return delay;

                currentVisibleCharacterIndex++;
            }
        }
    }
}