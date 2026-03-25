using BigProject.Player;
using UnityEngine;

namespace BigProject.Managers.CursorManager
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private Texture2D _pressedCursorTexture;
        [SerializeField] private Vector2 _pressedCursorHotspot;
        [SerializeField] private Texture2D _defaultCursorTexture;
        [SerializeField] private Vector2 _defaultCursorHotspot;
        [SerializeField] private CursorMode _cursorMode;

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
            SetCursor(_isPressing ? _pressedCursorTexture : _defaultCursorTexture,
                _isPressing ? _pressedCursorHotspot : _defaultCursorHotspot);
        }

        /// <summary>
        /// Sets a new cursor texture
        /// <param name="cursorTexture">Texture of the new cursor</param>
        /// <param name="hotspot">Cursor offset</param>
        /// <param name="isOverriding">If true cursor will be changed, regardless of it's current texture. If false, it will be changed only if it's default</param>
        /// </summary>
        public void SetCursor(Texture2D cursorTexture, Vector2 hotspot = default, bool isOverriding = false)
        {
            if(_currentTexture != _defaultCursorTexture && _currentTexture != _pressedCursorTexture && !isOverriding)
            {
                return;
            }

            _currentTexture = cursorTexture;
            Cursor.SetCursor(_currentTexture, hotspot, _cursorMode);
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