using BigProject.Managers;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

namespace BigProject.UI
{
    public class RunePanelUI : MonoBehaviour, IHUDWidget
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private List<RuneSlotUI> _runeSlots;
        [SerializeField] private List<Sprite> _backgroundSprites;
        private RunesSystem _runesSystem;
        private GameplayManager _gameplayManager;

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
            _runesSystem.OnQuestChanged += ChangeBackgroundBasedOnQuest;
            _runesSystem.OnCleared += OnCleared;
        }
        
        private void Start()
        {
            Assert.AreEqual(6, _runeSlots.Count, "You should add 6 rune slots in RunePanelUI");
            Assert.AreEqual(3, _backgroundSprites.Count, "You should add 3 background sprites in RunePanelUI");
        }

        private void OnDestroy()
        {
            _runesSystem.OnRuneAdded -= AddRune;
            _runesSystem.OnQuestChanged -= ChangeBackgroundBasedOnQuest;
            _runesSystem.OnCleared -= OnCleared;
        }

        private void AddRune(int runeId)
        {
            _runeSlots[runeId].ShowRune();
        }

        private void ChangeBackgroundBasedOnQuest(int questID)
        {
            if (questID < 1 || questID > 3)
            {
                Debug.LogError("You're trying to change runebar background using a wrong questID (must be in range [0; 2]). Background wasn't changed");
                return;
            }

            _backgroundImage.sprite = _backgroundSprites[questID - 1];
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
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