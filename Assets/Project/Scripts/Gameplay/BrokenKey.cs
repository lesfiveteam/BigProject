using UnityEngine;
using BigProject.Intercatable;
using BigProject.Systems;
using BigProject.Utilities;

namespace BigProject.Gameplay
{
    public class BrokenKey : MonoBehaviour, IInteractable
    {
        private string nameOfItem = "key_broken";
        private InventorySystem _inventory;

        public void Init(InventorySystem inventory)
        {
            _inventory = inventory;
            ExceptionUtilities.ThrowIfNull(_inventory, gameObject.name, "Inventory is null in brokenKey item!");
        }

        public void Interact()
        {
            _inventory.AddItemByName(nameOfItem);
            Destroy(gameObject);
        }
    }
}