using BigProject.Systems.Inventory;
using BigProject.Systems.Inventory.ItemsModifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image _image;
        [SerializeField] private Animator _itemAnimator;
        [SerializeField] private Material _itemShineMaterial;
        [SerializeField] private float _shineTime = 0.3f;
        private const string ITEM_POP_TRIGGER = "Pop";

        private const string NOTE_ANIM_TRIGGER = "Appear";
        private Animator _noteAnimator;
        
        private Transform _defaultParent;
        private Camera _camera;
        private Item _item;
        private GameObject _noteObject;

        public event Action<bool> OnStartDrag;

        public bool IsPlayingAnimation { get; set; } = false;

        private Material _currentMaterial;
        private Vector3 _currentPosition;

        private void Update()
        {
            if (!IsPlayingAnimation)
                return;

            _image.transform.position = _currentPosition;
        }

        private void Awake()
        {
            _defaultParent = transform.parent;
        }

        public Image GetImage()
        { 
            return _image;
        }

        public void SetImageTransformPosition(Vector3 currentPos)
        {
            _currentPosition = currentPos;
        }

        public IEnumerator PlayPopAnim()
        {
            _itemAnimator.SetTrigger(ITEM_POP_TRIGGER);
            yield return new WaitForSeconds(_shineTime);
            _currentMaterial.SetInt("_ShouldPlay", 1);    
            yield return new WaitForSeconds(_shineTime);
            _currentMaterial.SetInt("_ShouldPlay", 0);
            yield return new WaitForSeconds(_shineTime);
        }

        public void Hide()
        {
            if (IsPlayingAnimation)
            {
                StopAllCoroutines();
                _itemAnimator.ResetTrigger(ITEM_POP_TRIGGER);
                _currentMaterial.SetInt("_ShouldPlay", 0);
                transform.localScale = new(1f, 1f, 1f);
                IsPlayingAnimation = false;
            }
        }

        public void SetItem(Item item, Camera camera, Image noteImage, IReadOnlyList<ItemModifier> modifiers)
        {
            _image.sprite = item._itemSprite;
            _camera = camera;
            _item = item;

            _currentMaterial = Instantiate(_itemShineMaterial);
            _image.material = _currentMaterial;
            _currentMaterial.SetTexture("_MainText", item._itemSprite.texture);

            if (item._noteSprite != null && noteImage != null)
            {
                noteImage.sprite = item._noteSprite;
                _noteObject = noteImage.gameObject;
                _noteAnimator = _noteObject.GetComponent<Animator>();
            }

            AddModifiers(modifiers);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsPlayingAnimation)
                return;
            transform.SetParent(transform.root); //, false); - leads to change of scale
            transform.SetAsLastSibling();
            _image.raycastTarget = false;
            OnStartDrag?.Invoke(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsPlayingAnimation)
                return;

            transform.position = Mouse.current.position.ReadValue();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsPlayingAnimation)
                return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _camera.ScreenPointToRay(mousePosition);
            
            // hit something
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent<IUsesItem>(out IUsesItem interactableObject))
                {
                    if (interactableObject.DoesUseItem(_item))
                    { 
                        interactableObject.UseItem(_item);
                        OnStartDrag?.Invoke(false);
                        return;
                    }
                }
            }

            // didn't hit anything, returning item to its inventory slot
            transform.SetParent(_defaultParent);
            _image.raycastTarget = true;
            OnStartDrag?.Invoke(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsPlayingAnimation)
                return;

            if (_noteObject == null)
            {
                return;
            }

            _noteObject.SetActive(!_noteObject.activeInHierarchy);
            _noteAnimator.SetTrigger(NOTE_ANIM_TRIGGER);
        }

        private void AddModifiers(IReadOnlyList<ItemModifier> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return;
            }

            foreach (ItemModifier itemModifier in modifiers)
            {
                if (itemModifier.ItemSprite != null)
                {
                    AddModifierOnParent(itemModifier.ItemSprite, itemModifier.ItemUV, _image.transform, itemModifier.ItemScale);
                }

                if (itemModifier.NoteSprite != null && _noteObject != null)
                {
                    AddModifierOnParent(itemModifier.NoteSprite, itemModifier.NoteUV, _noteObject.transform, itemModifier.NoteScale);
                }
            }
        }

        /// <summary>
        /// Add modifier game object with image as child.
        /// </summary>
        /// <param name="sprite">Modifier sprite.</param>
        /// <param name="UV">UV coords from 0 to 1.</param>
        /// <param name="parent">Parent transform.</param>
        private void AddModifierOnParent(Sprite sprite, Vector2 UV, Transform parent, float scale)
        {
            GameObject modifierObj = new GameObject("Modifier", typeof(RectTransform), typeof(Image));
            modifierObj.transform.SetParent(parent, false);
            RectTransform rect = modifierObj.GetComponent<RectTransform>();
            Vector2 anchorPos = new Vector2(UV.x, 1f - UV.y);
            rect.pivot = new Vector2(0, 1);
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = new(scale, scale);
            Image image = modifierObj.GetComponent<Image>();
            image.sprite = sprite;
        }

        private void OnDestroy()
        {
            if (_noteObject != null)
            {
                _noteObject = null;
            }
        }
    }
}