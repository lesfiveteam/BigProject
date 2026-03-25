using BigProject.Managers.CursorManager;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.Intercatable.HighlightedObjects
{
    public class CursorChangingEffect : MonoBehaviour, IHighlightEffect, IPressableEffect
    {
        [SerializeField] protected Texture2D _highlightCursorTexture;
        [Tooltip ("Should be equal to half the size of a sprite")]
        [SerializeField] protected Vector2 _highlightCursorHotspot = Vector2.zero; 
        [SerializeField] protected Texture2D _pressedCursorTexture;
        [Tooltip ("Should be equal to half the size of a sprite")]
        [SerializeField] protected Vector2 _pressedCursorHotspot = Vector2.zero; 

        protected CursorManager _cursorManager;

        public void Init(CursorManager cursorManager)
        {
            _cursorManager = cursorManager;
            ExceptionUtilities.ThrowIfNull(_cursorManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "CursorManager"));
        }

        public virtual void EnableEffect()
        {
            _cursorManager.SetCursor(_highlightCursorTexture, _highlightCursorHotspot);
        }

        public void DisableEffect()
        {
            _cursorManager.ResetToDefault();
        }

        public virtual void SetPressableEffect()
        {
            _cursorManager.SetCursor(_pressedCursorTexture, _pressedCursorHotspot, true);
        }
    }
}