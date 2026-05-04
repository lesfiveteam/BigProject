using BigProject.Managers;
using BigProject.Systems.Inventory;
using BigProject.Utilities;
using UnityEngine;
using UnityEngine.Localization;

namespace BigProject.Gameplay.VillageChurchQuest
{
    public class QuestActions : MonoBehaviour
    {
        private InventorySystem _inventory;

        [SerializeField]
        private string _noteOne, _noteTwo, _noteThree;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _firstEnterRemark;

        public void Init(InventorySystem inventory)
        {
            _inventory = inventory;
            ExceptionUtilities.ThrowIfNull(_inventory, string.Format(gameObject.name, "Inventory System"));
        }

        public void AddNoteTwo()
        {
            _inventory.RemoveItemByName(_noteOne);
            _inventory.AddItemByName(_noteTwo);
        }

        public void AddNoteThree()
        {
            _inventory.RemoveItemByName(_noteTwo);
            _inventory.AddItemByName(_noteThree);
        }

        // При первом ЛКМ в деревне
        public void FirstVillageStepAction()
        {
            ReplicaManager.ShowReplica(_firstEnterRemark);
        }
    }
}