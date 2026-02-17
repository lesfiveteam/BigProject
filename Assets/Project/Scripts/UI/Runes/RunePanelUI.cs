using BigProject.Managers;
using BigProject.Systems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.UI
{
    public class RunePanelUI : MonoBehaviour
    {
        [SerializeField] private List<RuneSlotUI> _runeSlots;
        private RunesSystem _runesSystem;

        public void Init(RunesSystem runesSystem)
        {
            if (runesSystem == null)
            {
                GameLogManager.Error("RunesSystem in RunePanelUI was set to null");
                throw new System.ArgumentNullException(nameof(runesSystem), "RunesSystem cannot be null");
            }

            _runesSystem = runesSystem;
        }
        
        private void Start()
        {
            Assert.AreEqual(3, _runeSlots.Count, "You should add 3 rune slots in RunePanelUI");
        }

        private void OnEnable()
        {
            _runesSystem.OnRuneAdded += AddRune;
        }

        private void OnDisable()
        {
            _runesSystem.OnRuneAdded -= AddRune;
        }

        private void AddRune(int runeId)
        {
            _runeSlots[runeId].ShowRune();
        }
    }
}