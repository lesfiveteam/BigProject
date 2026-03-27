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

        public void Init(Transform startPos, Transform goalPos, Sprite sprite, Image boardBorders, int id, int segmentID)
        {
            _startPos = startPos;
            _goalPos = goalPos;
            _image.sprite = sprite;
            _boardBorders = boardBorders;
            _id = id;
            _segmentID = segmentID;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isOnRightPlace) return;

            _isMoving = true;
            Vector3 mousePos = Mouse.current.position.ReadValue();
            _offset = transform.position - mousePos;
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
            if (!ShardBoardOverlapChecker.IsShardInsideBoard(_image, _boardBorders))
            {
                transform.position = _startPos.position;
            }

            if (Vector3.Distance(transform.position, _goalPos.position) <= EPSILON)
            {
                transform.position = _goalPos.position;
                _isOnRightPlace = true;
                OnShardPlacedCorrectly?.Invoke(_id);
            }
        }
    }
}