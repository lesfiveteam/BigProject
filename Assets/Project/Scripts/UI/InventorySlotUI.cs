using BigProject.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private GameObject _inventoryItemPrefab;
        private InventoryItemUI _inventoryItemUI;

        public void SetSlot(Item item, Camera camera, Image noteImage = null)
        {
            if (_inventoryItemUI != null)
                ClearSlot();

            _inventoryItemUI = Instantiate(_inventoryItemPrefab, this.transform).GetComponent<InventoryItemUI>();
            _inventoryItemUI.SetItem(item, camera, noteImage);
        }

        public void ClearSlot()
        {
            if (_inventoryItemUI == null)
                return;

            Destroy(_inventoryItemUI.gameObject);
            _inventoryItemUI = null;
        }
    }
}