using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BigProject.UI
{
    public class ShardUI : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private Image _boardBorders;
        [SerializeField] private Image _image;

        private Transform _startPos;
        private Transform _goalPos;

        private bool _isMoving;
        private bool _isOnRightPlace;
        private Vector3 _offset;
        private int _id;
        private int _segmentID;

        // Maximum distance from shard to goal position for it to be sticked
        private const float EPSILON = 10.0f;

        public event Action<int> OnShardPlacedCorrectly;

        public int ID => _id;
        public int SegmentID => _segmentID;

        public void Init(Transform startPos, Transform goalPos, Sprite sprite, Image boardBorders, int id, int segmentID, Vector2 imgSize, bool isPlaced = false)
        {
            if (startPos == null || goalPos == null)
            {
                Debug.LogError("ShardUI.Init: startPos or goalPos is NULL!");
                return;
            }

            if (_image == null)
            {
                Debug.LogError("ShardUI.Init: Image component is NULL!");
                return;
            }

            if (sprite == null)
            {
                Debug.LogWarning($"Shard {id} has no sprite!");
            }

            _startPos = startPos;
            _goalPos = goalPos;
            _image.sprite = sprite;
            _boardBorders = boardBorders;
            _id = id;
            _segmentID = segmentID;

            _isOnRightPlace = isPlaced;
            transform.position = isPlaced ? _goalPos.position : _startPos.position;

            _image.rectTransform.sizeDelta = imgSize;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isOnRightPlace) return;

            _isMoving = true;
            Vector3 mousePos = Mouse.current.position.ReadValue();
            _offset = transform.position - mousePos;
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isMoving)
            {
                transform.position = Mouse.current.position.ReadValue();
                transform.position += _offset;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isMoving = false;

            if (_startPos == null || _goalPos == null)
            {
                Debug.LogError($"Shard {_id}: startPos or goalPos is NULL!");
                return;
            }

            if (_boardBorders == null)
            {
                Debug.LogError($"Shard {_id}: boardBorders is NULL!");
                return;
            }

            if (!ShardBoardOverlapChecker.IsShardInsideBoard(_image, _boardBorders))
            {
                transform.position = _startPos.position;
            }

            if (Vector3.Distance(transform.position, _goalPos.position) <= EPSILON)
            {
                if (_isOnRightPlace)
                {
                    Debug.LogWarning($"Shard {_id} already placed but triggered again!");
                    return;
                }

                transform.position = _goalPos.position;
                _isOnRightPlace = true;
                transform.SetAsLastSibling();
                OnShardPlacedCorrectly?.Invoke(_id);
            }
        }
    }
}