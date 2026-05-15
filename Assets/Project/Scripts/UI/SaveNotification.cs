using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.UI
{
    public class SaveNotification : MonoBehaviour, IHUDWidget
    {
        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private string _saveText;
        [SerializeField]
        private float _timePerDot;
        [SerializeField]
        private int _dotsNumber;
        [SerializeField]
        private int _iterationsNumber;
        [SerializeField]
        private GameObject _view;

        private WaitForSeconds _delay;
        private ProgressManager _progressManager;
        private GameplayManager _gameplayManager;

        private void Awake()
        {
            Assert.IsNotNull(_text, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "SaveNotification", "TMP_Text"));
            Assert.IsNotNull(_view, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "SaveNotification", "View GameObject"));
        }

        public void Init(ProgressManager progressManager, GameplayManager gameplayManager)
        {
            _progressManager = progressManager;
            _gameplayManager = gameplayManager;
            ExceptionUtilities.ThrowIfNull(_progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SaveNotification", "ProgressManager"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SaveNotification", "GameplayManager"));
        }

        public void Show()
        {
            _delay = new(_timePerDot);
            StartCoroutine(PlayDots());
            _view.SetActive(true);
        }

        public void Hide()
        {
            StopAllCoroutines();
            _delay = null;
            _view.SetActive(false);
        }

        private IEnumerator PlayDots()
        {
            for (int i = 0; i < _iterationsNumber; i++)
            {
                _text.text = _saveText;

                for (int j = 0; j < _dotsNumber + 1; j++)
                {
                    yield return _delay;
                    _text.text += '.';
                }
            }

            Hide();
        }

        private void OnProgressSaved()
        {
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            yield return new WaitUntil(() => _gameplayManager.State == GameplayState.Play);
            Show();
        }

        private void OnEnable()
        {
            _progressManager.ProgressSaved += OnProgressSaved;
        }

        private void OnDisable()
        {
            _progressManager.ProgressSaved -= OnProgressSaved;
        }
    }
}
