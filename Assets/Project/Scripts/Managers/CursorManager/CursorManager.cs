using BigProject.Player;
using System;
using System.Linq;
using UnityEngine;

namespace BigProject.Managers.CursorManager
{
    public enum CursorType
    {
        Default,
        Dialogue,
        Outline,
        Hand,
        Door,
    }


    [Serializable]
    public struct CursorData
    {
        public CursorType CursorType;
        public Texture2D DefaultCursorTexture;
        public Vector2 DefaultCursorHotspot;
        public Texture2D PressedCursorTexture;
        public Vector2 PressedCursorHotspot;
    }

    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private CursorType _defaultCursorType;
        [SerializeField] private Texture2D _pressedCursorTexture;
        [SerializeField] private Texture2D _defaultCursorTexture;
        [SerializeField] private CursorsConfig _cursorsConfig;

        private PlayerInputHandler _inputHandler;
        private Texture2D _currentTexture;
        private bool _isPressing;

        private void Awake()
        {
            ResetToDefault();
        }

        public void Init(PlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }

        private void OnEnable()
        {
            _inputHandler.Click += OnClick;
            _inputHandler.ClickRelease += OnClickRelease;
        }

        private void OnDisable()
        {
            _inputHandler.Click -= OnClick;
            _inputHandler.ClickRelease -= OnClickRelease;
        }

        private void OnClick()
        {
            _isPressing = true;
            SetDefaultCursor();
        }

        private void OnClickRelease()
        {
            _isPressing = false;
            SetDefaultCursor();
        }

        private void SetDefaultCursor()
        {
            SetCursor(_defaultCursorType, _isPressing);
        }

        /// <summary>
        /// Sets a new cursor texture
        /// <param name="cursorTexture">Texture of the new cursor</param>
        /// <param name="hotspot">Cursor offset</param>
        /// <param name="isOverriding">If true cursor will be changed, regardless of it's current texture. If false, it will be changed only if it's default</param>
        /// </summary>
        public void SetCursor(CursorType cursorType, bool isPressed = false, bool isOverriding = false)
        {
            if(_currentTexture != _defaultCursorTexture && _currentTexture != _pressedCursorTexture && !isOverriding)
            {
                return;
            }

            CursorData newCursorData = _cursorsConfig.CursorDatas.First(cursorData => cursorData.CursorType == cursorType);
            _currentTexture = isPressed ? newCursorData.PressedCursorTexture : newCursorData.DefaultCursorTexture;
            Vector2 hotSpot = isPressed ? newCursorData.PressedCursorHotspot : newCursorData.DefaultCursorHotspot;
            Cursor.SetCursor(_currentTexture, hotSpot, CursorMode.Auto);
        }

        /// <summary>
        /// Sets cursor to a default texture
        /// </summary>
        public void ResetToDefault()
        {
            _currentTexture = _defaultCursorTexture;
            SetDefaultCursor();
        }
    }
}