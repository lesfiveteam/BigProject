using BigProject.Managers;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class InventoryUI : MonoBehaviour, IHUDWidget
    {
        [SerializeField] private List<InventorySlotUI> _inventorySlots;
        [SerializeField] private Image _noteImage;
        [SerializeField] private float _addAnimationStepTime = 0.1f;
        private InventorySystem _inventorySystem;
        private int _previousHeldItemsCount = 0;
        private const string NOTE_ANIM_TRIGGER = "Appear";
        private Animator _noteAnimator;
        private Transform _playerTransform;

        public void Init(InventorySystem inventorySystem, Transform playerTransform)
        {
            if (inventorySystem == null)
            {
                GameLogManager.Error("InventorySystem in InventoryUI was set to null");
                throw new System.ArgumentNullException(nameof(inventorySystem), "InventorySystem cannot be null");
            }

            _playerTransform = playerTransform;
            _inventorySystem = inventorySystem;
            _inventorySystem.OnInventoryUpdated += UpdateInventory;
        }

        private void Start()
        {
            Assert.AreEqual(5, _inventorySlots.Count, "Less than 5 inventory slots were added in InventoryUI");
            Assert.IsNotNull(_noteImage, "Note image was not initialised for InventoryUI");
            if (_noteImage != null)
            {
                _noteAnimator = _noteImage.gameObject.GetComponent<Animator>();
                Assert.IsNotNull(_noteAnimator, "Note doesn't haave an animator in InventoryUI");
            }
            Assert.IsNotNull(_inventorySystem, "Inventory System was not initialised for InventoryUI");
        }

        private void OnDestroy()
        {
            _inventorySystem.OnInventoryUpdated -= UpdateInventory;
        }

        private void UpdateInventory()
        {
            bool hasNote = false;
            List<Item> heldItems = _inventorySystem.GetAllHeldItems();

            // Clear note because of all items will be recreated regardless of their presence.
            // For not duplicate modifiers.
            foreach (Transform child in _noteImage.gameObject.transform)
            {
                Destroy(child.gameObject);
            }

            if (heldItems.Count == 0)
            {
                for (int i = 0; i < _inventorySlots.Count; i++)
                {
                    _inventorySlots[i].ClearSlot();
                }
                
                _noteImage.gameObject.SetActive(false);
                _previousHeldItemsCount = 0;
                return;
            }

            //if item was added
            if (_previousHeldItemsCount < heldItems.Count && this.gameObject.activeInHierarchy)
            {
                for (int i = 0; i < heldItems.Count-1; i++)
                {
                    _inventorySlots[i].SetSlot(heldItems[i], Camera.main, _noteImage, _inventorySystem.GetHeldItemModifiers(heldItems[i]._name));

                    if (heldItems[i]._noteSprite != null)
                    {
                        hasNote = true;
                    }
                }

                _inventorySlots[heldItems.Count - 1].SetSlot(heldItems[heldItems.Count - 1], Camera.main, _noteImage, _inventorySystem.GetHeldItemModifiers(heldItems[heldItems.Count - 1]._name));

                if (heldItems[heldItems.Count - 1]._noteSprite != null)
                {
                    hasNote = true;
                }

                Vector2 startPos = heldItems[heldItems.Count - 1]._isAddedAtMiniGame ?
                        Mouse.current.position.ReadValue()  
                        : Camera.main.WorldToScreenPoint(_playerTransform.position + _playerTransform.forward * 0.5f);

                Vector2 targetpos = _inventorySlots[heldItems.Count - 1].transform.position;
                _inventorySlots[heldItems.Count - 1].GetItemImage().gameObject.SetActive(false);
                StartCoroutine(_inventorySlots[heldItems.Count - 1].PlayAddToSlotAnimation(startPos, targetpos, _addAnimationStepTime));
            }
            else //it item was removed or we just need to update inventory
            {
                for (int i = 0; i < heldItems.Count; i++)
                {
                    _inventorySlots[i].SetSlot(heldItems[i], Camera.main, _noteImage, _inventorySystem.GetHeldItemModifiers(heldItems[i]._name));

                    if (heldItems[i]._noteSprite != null)
                    {
                        hasNote = true;
                    }
                }

                for (int i = heldItems.Count; i < _inventorySlots.Count; i++)
                {
                    _inventorySlots[i].ClearSlot();
                }
            }

            _previousHeldItemsCount = heldItems.Count;

            // hides note image if there is no corresponding item
            if (!hasNote)
            {
                _noteImage.gameObject.SetActive(false);
            }
        }

        public void SetNoteVisibility(bool isVisible)
        {
            if (_noteImage.sprite != null)
            {
                _noteImage.gameObject.SetActive(isVisible);

                if (isVisible && _noteAnimator != null)
                {
                    _noteAnimator.SetTrigger(NOTE_ANIM_TRIGGER);
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            SetNoteVisibility(false);
        }
    }
}