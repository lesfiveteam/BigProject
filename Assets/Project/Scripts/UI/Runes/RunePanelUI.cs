using BigProject.Managers;
using BigProject.Systems.Inventory;
using BigProject.Systems.HUD;
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
        [SerializeField] private List<Sprite> _backgroundSprites;
        private RunesSystem _runesSystem;

        public void Init(RunesSystem runesSystem)
        {
            if (runesSystem == null)
            {
                GameLogManager.Error("RunesSystem in RunePanelUI was set to null");
                throw new System.ArgumentNullException(nameof(runesSystem), "RunesSystem cannot be null");
            }

            _runesSystem = runesSystem;
            _runesSystem.OnRuneAdded += AddRune;
            _runesSystem.OnQuestChanged += ChangeBackgroundBasedOnQuest;
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
        }

        private void AddRune(int runeId)
        {
            _runeSlots[runeId].ShowRune();
        }

        private void ChangeBackgroundBasedOnQuest(int questID)
        {
            if (questID < 0 || questID >= 3)
            {
                Debug.LogError("You're trying to change runebar background using a wrong questID (must be in range [0; 2]). Background wasn't changed");
                return;
            }

            _backgroundImage.sprite = _backgroundSprites[questID];
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}