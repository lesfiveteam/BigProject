using BigProject.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image _image;

        private Transform _defaultParent;
        private Camera _camera;
        private Item _item;
        private GameObject _noteObject;
        private bool _isNoteOpened;

        void Awake()
        {
            _defaultParent = transform.parent;
        }

        public void SetItem(Item item, Camera camera, Image noteImage)
        {
            _image.sprite = item._itemSprite;
            _camera = camera;
            _item = item;
            if (item._noteSprite != null)
            {
                noteImage.sprite = item._noteSprite;
                _noteObject = noteImage.gameObject;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.SetParent(transform.root, false);
            transform.SetAsLastSibling();
            _image.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Mouse.current.position.ReadValue();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _camera.ScreenPointToRay(mousePosition);
            
            //попали во что-то
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent<IUsesItem>(out IUsesItem interactableObject))
                {
                    if (interactableObject.DoesUseItem(_item))
                    { 
                        interactableObject.UseItem(_item);
                        return;
                    }
                }
            }

            //не попали в нужный объект, возвращаем предмет на его позицию в инвентаре
            transform.SetParent(_defaultParent);
            _image.raycastTarget = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_noteObject == null)
                return;
            _isNoteOpened = !_isNoteOpened;
            _noteObject.SetActive(_isNoteOpened);
        }

        private void OnDestroy()
        {
            _isNoteOpened = false;
            if (_noteObject != null)
            {
                _noteObject.SetActive(false);
                _noteObject = null;
            }
        }
    }
}