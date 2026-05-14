using BigProject.Systems.Inventory;
using BigProject.Systems.Inventory.ItemsModifiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject _inventoryItemPrefab;
        [SerializeField] private Image _slotImage;
        
        [Header("Спрайты для слота")]
        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _hoverSprite;
        
        private InventoryItemUI _inventoryItemUI;
        private bool _isEmpty = true;
        private bool _isSelected = false;

        public void SetSlot(Item item, Camera camera, Image noteImage = null, IReadOnlyList<ItemModifier> modifiers = null)
        {
            if (_inventoryItemUI != null)
                ClearSlot();

            _inventoryItemUI = Instantiate(_inventoryItemPrefab, this.transform).GetComponent<InventoryItemUI>();
            _inventoryItemUI.SetItem(item, camera, noteImage, modifiers);
            _inventoryItemUI.OnStartDrag += SlotSelected;
            _isEmpty = false;
            _isSelected = false;
        }

        public InventoryItemUI GetInventoryItemUI() { return _inventoryItemUI; }

        public void ClearSlot()
        {
            if (_inventoryItemUI == null)
                return;

            Destroy(_inventoryItemUI.gameObject);
            _inventoryItemUI = null;
            _isEmpty = true;
            _isSelected = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isSelected)
                return;

            _slotImage.sprite = _hoverSprite;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected)
                return;
            
            _slotImage.sprite = _defaultSprite;
        }

        private void SlotSelected(bool slotSelected)
        {
            if (slotSelected)
            {
                if (_isEmpty)
                    return;

                _isSelected = true;
                _slotImage.sprite = _selectedSprite;
            }
            else
            {
                _isSelected = false;
                _slotImage.sprite = _defaultSprite;
            }
        }

        public Image GetItemImage()
        { 
            return _inventoryItemUI.GetImage();
        }

        public IEnumerator PlayAddToSlotAnimation(Vector2 startPosition, Vector2 targetPosition, float animStepTime)
        {
            _inventoryItemUI.SetIsPlayingAnimation(true);
            Image spriteImage = _inventoryItemUI.GetImage();
            spriteImage.gameObject.SetActive(true);
            _inventoryItemUI.SetImageTransformPosition(startPosition);

            yield return _inventoryItemUI.PlayPopAnim();

            float t = 0.0f;

            while (t <= 1.1f)
            {
                Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
                _inventoryItemUI.SetImageTransformPosition(currentPosition);
                t += 0.05f;
                yield return new WaitForSeconds(animStepTime * 0.01f);
            }
            _inventoryItemUI.SetIsPlayingAnimation(false);
        }
    }
}