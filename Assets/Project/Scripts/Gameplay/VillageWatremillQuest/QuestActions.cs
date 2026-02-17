using BigProject.Gameplay.Watermill;
using BigProject.Managers;
using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.VillageWatermillQuest
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private int _noteItemId;
        [SerializeField]
        private int _brokenLeverItemId;
        [SerializeField]
        private int _repairedLeverItemId;
        [SerializeField]
        private GameObject _miller;
        [SerializeField]
        private GameObject _chests;
        [SerializeField]
        private Vector3 _millerFinalPosition;
        [SerializeField]
        private GearsHandler _millWheelHandler;
        [SerializeField]
        private GameObject _runeBar;

        private InventorySystem _inventory;
        private RunesSystem _runes;

        private void Start()
        {
            _inventory = ServiceLocator.GetService<InventorySystem>();
            Assert.IsNotNull(_inventory, $"{gameObject.name} unable to get inventory system.");
        }

        public void GetWatermillNote()
        {
            GameLogManager.Info("Add mill sketch to inventory.");
            _inventory.AddItemByItemID(_noteItemId);
            //ServiceLocator.GetService<Journal> add note
            GameLogManager.Info("Add note about mill to journal.");
        }

        public void GetRepairedLever()
        {
            GameLogManager.Info("Remove broken lever from inventory.");
            _inventory.RemoveItemById(_brokenLeverItemId);
            GameLogManager.Info("Add repaired lever to inventory.");
            _inventory.AddItemByItemID(_repairedLeverItemId);
        }

        public void DespawnMiller()
        {
            GameLogManager.Info("Despawn miller from scene.");
            _miller.SetActive(false);
        }

        public void SpawnMiller()
        {
            GameLogManager.Info("Move miller to quest final position and spawn chests.");
            _chests.SetActive(true);
            _miller.transform.position = _millerFinalPosition;
        }

        public void RotateMillWheelOn()
        {
            GameLogManager.Info("Switch rotation of mill wheel on.");
            _millWheelHandler.enabled = true;
        }

        public void GetRune()
        {
            _runeBar.SetActive(true);
            _runes.AddRune();
        }
    }
}