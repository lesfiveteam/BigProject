using BigProject.Managers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class RunePanelUI : MonoBehaviour, IHUDWidget
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private List<RuneSlotUI> _runeSlots;
        [SerializeField] private List<BackgroundData> _backgrounds;
        [SerializeField] private GlintController _glintController;
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Canvas _runesCanvas;
        [SerializeField] private Material _runeMaterial;

        private RunesSystem _runesSystem;
        private GameplayManager _gameplayManager;
        private bool _isVisible;
        private bool _isPlayable;
        private Vector3 _startScale;
        private int _openedSegmentsCount;
        private bool _isAssembled;
        private Vector2 _runesPosition;

        private readonly Vector2 FIRST_APPEARANCE_OFFSET = new(-200f, 0f);
        private readonly Vector2 RUNE_INIT_SIZE = new(15f, 15f);
        private readonly Vector2 RUNE_EACH_OFFSET = new(30f, 10f);
        private const float FIRST_APPEARANCE_TIME = 0.5f;
        private const float GET_RUNES_SHAKE_STRENGTH = 0.5f;
        private const int GET_RUNES_SHAKE_VIBRATO = 1;
        private const float GET_RUNES_SHAKE_TIME = 0.2f;
        private const float GET_RUNES_CHANGE_BACK_TIME = 0.7f;
        private const float RUNE_SCALE = 5f;
        private const float RUNES_SCALE_TIME = 1f;
        private const float RUNES_FLY_TIME = 1f;

        [Serializable]
        private struct BackgroundData
        {
            public RunesJigsawUI.BackingImage backImage;
            public Vector3 mask;
            public List<Sprite> runes;
        }

        public void Init(RunesSystem runesSystem, GameplayManager gameplayManager)
        {
            if (runesSystem == null)
            {
                GameLogManager.Error("RunesSystem in RunePanelUI was set to null");
                throw new System.ArgumentNullException(nameof(runesSystem), "RunesSystem cannot be null");
            }

            _runesSystem = runesSystem;
            _gameplayManager = gameplayManager;
            _runesSystem.OnRuneAdded += AddRune;
            _runesSystem.OnSegmentUnlocked += ChangeBackground;
            _runesSystem.OnCleared += OnCleared;
        }

        public void SetRunesOnScreenPosition(Vector2 position)
        {
            _runesPosition = position;
        }
        
        private void Awake()
        {
            Assert.AreEqual(6, _runeSlots.Count, "You should add 6 rune slots in RunePanelUI");
            Assert.AreEqual(3, _backgrounds.Count, "You should add 3 background sprites in RunePanelUI");
            Assert.IsNotNull(_glintController, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "GlintController"));
            Assert.IsNotNull(_rect, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "RectTransform"));
            _backgrounds.Sort((a, b) => b.backImage.unlockedSegmentsThreshold.CompareTo(a.backImage.unlockedSegmentsThreshold));
            _startScale = _rect.localScale;
        }

        private void OnDestroy()
        {
            _runesSystem.OnRuneAdded -= AddRune;
            _runesSystem.OnSegmentUnlocked -= ChangeBackground;
            _runesSystem.OnCleared -= OnCleared;
        }

        private void AddRune(int runeId)
        {
            _runeSlots[runeId].ShowRune();
            _runeSlots[runeId].ShowRune();
            _isAssembled = _openedSegmentsCount <= _runeSlots.FindAll(x => x.IsActive).Count;

            if (_isAssembled)
            {
                _glintController.Stop();
            }
        }

        private void ChangeBackground(int segmentsCount)
        {
            StopAllCoroutines();
            _openedSegmentsCount = segmentsCount;

            foreach (BackgroundData background in _backgrounds)
            {
                if (_openedSegmentsCount >= background.backImage.unlockedSegmentsThreshold)
                {
                    if (!_isPlayable)
                    {
                        _isPlayable = true;

                        if (_isVisible)
                        {
                            Show();
                            _glintController.Stop();
                            StartCoroutine(FirstAppearanceRoutine(background));
                        }
                    }
                    else if (_isVisible)
                    {
                        _glintController.Stop();
                        RunesFly(background);
                    }
                    else
                    {
                        Vector3 mask = background.mask;
                        _glintController.SetMask(mask.x > 0f, mask.y > 0f, mask.z > 0f);
                        _backgroundImage.sprite = background.backImage.sprite;
                    }

                    return;
                }
            }

            _isPlayable = false;
        }

        public void Show()
        {
            _isVisible = true;

            if (_isPlayable)
            {
                gameObject.SetActive(true);

                if (!_isAssembled)
                {
                    _glintController.Play();
                }
            }
        }

        public void Hide()
        {
            _isVisible = false;
            gameObject.SetActive(false);
            _glintController.Stop();
        }

        public void OpenJigsawPanel()
        {
            _gameplayManager.ChangeState(GameplayState.RunesJagsaw);
        }

        private void OnCleared()
        {
            _runeSlots.ForEach(x => x.HideRune());
        }

        private IEnumerator FirstAppearanceRoutine(BackgroundData background)
        {
            _rect.anchoredPosition -= FIRST_APPEARANCE_OFFSET;
            _rect.DOAnchorPos(FIRST_APPEARANCE_OFFSET, FIRST_APPEARANCE_TIME).SetRelative();
            yield return new WaitForSeconds(FIRST_APPEARANCE_TIME);
            RunesFly(background);
        }

        private IEnumerator GetRunesRoutine(BackgroundData background)
        {
            Vector3 mask = background.mask;
            _glintController.SetMask(mask.x > 0f, mask.y > 0f, mask.z > 0f);
            _rect.DOShakeScale(GET_RUNES_SHAKE_TIME, GET_RUNES_SHAKE_STRENGTH, GET_RUNES_SHAKE_VIBRATO, 0f).OnComplete(() =>
                _rect.localScale = _startScale);
            yield return new WaitForSeconds(GET_RUNES_SHAKE_TIME);
            _glintController.Play();
            yield return new WaitForSeconds(GET_RUNES_CHANGE_BACK_TIME);
            _backgroundImage.sprite = background.backImage.sprite;
        }

        private void RunesFly(BackgroundData background)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_runesCanvas.transform as RectTransform, _runesPosition,
                _runesCanvas.worldCamera, out Vector2 localPoint);

            foreach (Sprite rune in background.runes)
            {
                GameObject go = new GameObject("Rune");
                go.transform.SetParent(_runesCanvas.transform, false);
                Image img = go.AddComponent<Image>();
                img.rectTransform.sizeDelta = RUNE_INIT_SIZE;
                img.sprite = rune;
                img.rectTransform.anchoredPosition = localPoint;
                img.raycastTarget = false;
                img.material = new Material(_runeMaterial);
                img.material.SetFloat("_StartTime", Time.timeSinceLevelLoad);

                img.rectTransform.DOScale(RUNE_SCALE, RUNES_SCALE_TIME).OnComplete(() => 
                    img.rectTransform.DOMove(GetRuneTarget(), RUNES_FLY_TIME).SetEase(Ease.InQuad).OnComplete(() =>
                        {
                            Destroy(go);
                            StartCoroutine(GetRunesRoutine(background));
                        }));

                localPoint += RUNE_EACH_OFFSET;
            }
        }

        private Vector3 GetRuneTarget()
        {
            Vector3[] corners = new Vector3[4];
            _rect.GetWorldCorners(corners);
            return (corners[0] + corners[2]) / 2f;
        }
    }
}