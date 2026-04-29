using BigProject.Managers.CursorManager;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.Intercatable.HighlightedObjects
{
    public class CursorChangingEffect : MonoBehaviour, IHighlightEffect, IPressableEffect
    {
        [SerializeField] protected CursorType _cursorType;

        protected CursorManager _cursorManager;

        public void Init(CursorManager cursorManager)
        {
            _cursorManager = cursorManager;
            ExceptionUtilities.ThrowIfNull(_cursorManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "CursorManager"));
        }

        public virtual void EnableEffect()
        {
            _cursorManager.SetCursor(_cursorType);
        }

        public void DisableEffect()
        {
            _cursorManager.ResetToDefault();
        }

        public virtual void SetPressableEffect()
        {
            _cursorManager.SetCursor(_cursorType, true, true);
        }
    }
}