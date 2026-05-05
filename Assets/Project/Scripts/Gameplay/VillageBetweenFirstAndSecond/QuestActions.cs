using BigProject.Managers.CutsceneManager;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace BigProject.Gameplay.VillageBetweenFirstAndSecond
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private AssetReferenceT<TimelineAsset> _startCutscene;
        [SerializeField]
        private string _noteItemName;

        private InventorySystem _inventory;
        private CutsceneManager _cutsceneManager;

        private void Start()
        {
            Assert.IsNotNull(_inventory, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "InventorySystem"));
            Assert.IsNotNull(_cutsceneManager, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "CutsceneManager"));
        }

        public void Init(InventorySystem inventory, CutsceneManager cutsceneManager)
        {
            _inventory = inventory;
            _cutsceneManager = cutsceneManager;
        }

        public void PlayCutscene()
        {
            _cutsceneManager.Play(_startCutscene);
        }

        public void GetWatermillNote()
        {
            _inventory.AddItemByName(_noteItemName);
        }
    }
}