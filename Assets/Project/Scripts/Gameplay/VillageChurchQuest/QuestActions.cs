using BigProject.Systems.Inventory;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.Gameplay.VillageChurchQuest
{
    public class QuestActions : MonoBehaviour
    {
        private InventorySystem _inventory;

        [SerializeField]
        private string _noteOne, _noteTwo, _noteThree;

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
    }
}