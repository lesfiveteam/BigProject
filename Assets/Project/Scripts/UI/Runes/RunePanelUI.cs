using BigProject.Managers;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
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
        [SerializeField] private List<RunesJigsawUI.BackingImage> _backgrounds;
        private RunesSystem _runesSystem;
        private GameplayManager _gameplayManager;
        private bool _isVisible;
        private bool _isPlayable;

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
        
        private void Awake()
        {
            Assert.AreEqual(6, _runeSlots.Count, "You should add 6 rune slots in RunePanelUI");
            Assert.AreEqual(3, _backgrounds.Count, "You should add 3 background sprites in RunePanelUI");
            _backgrounds.Sort((a, b) => b.unlockedSegmentsThreshold.CompareTo(a.unlockedSegmentsThreshold));
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
        }

        private void ChangeBackground(int segmentsCount)
        {
            foreach (RunesJigsawUI.BackingImage background in _backgrounds)
            {
                if (segmentsCount >= background.unlockedSegmentsThreshold)
                {
                    _backgroundImage.sprite = background.sprite;

                    if (!_isPlayable)
                    {
                        _isPlayable = true;

                        if (_isVisible)
                        {
                            Show();
                        }
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
            }
        }

        public void Hide()
        {
            _isVisible = false;
            gameObject.SetActive(false);
        }

        public void OpenJagsawPanel()
        {
            _gameplayManager.ChangeState(GameplayState.RunesJagsaw);
        }

        private void OnCleared()
        {
            _runeSlots.ForEach(x => x.HideRune());
        }
    }
}