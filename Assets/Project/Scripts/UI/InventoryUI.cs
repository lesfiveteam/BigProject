using BigProject.Systems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private List<InventorySlotUI> _inventorySlots;
        [SerializeField] private Camera _camera;
        [SerializeField] private Image _noteImage;

        private void OnEnable()
        {
            InventorySystem.Instance.OnInventoryUpdated += UpdateInventory;
        }

        private void OnDisable()
        {
            InventorySystem.Instance.OnInventoryUpdated -= UpdateInventory;
        }

        private void UpdateInventory()
        {
            List<Item> heldItems = InventorySystem.Instance.GetAllHeldItems();

            if (heldItems.Count == 0)
            {
                for (int i = 0; i < _inventorySlots.Count; i++)
                    _inventorySlots[i].ClearSlot();
                return;
            }

            for (int i = 0; i < heldItems.Count; i++)
            {
                _inventorySlots[i].SetSlot(heldItems[i], _camera, _noteImage);
            }

            for (int i = heldItems.Count; i < _inventorySlots.Count; i++)
            {
                _inventorySlots[i].ClearSlot();
            }
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