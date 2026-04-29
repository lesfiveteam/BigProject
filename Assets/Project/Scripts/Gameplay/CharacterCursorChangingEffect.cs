using BigProject.Intercatable.HighlightedObjects;
using BigProject.NPC;
using UnityEngine;

namespace BigProject.Gameplay
{
    public class CharacterCursorChangingEffect : CursorChangingEffect
    {
        [SerializeField]
        private DialogNPC _dialog;

        public override void EnableEffect()
        {
            if (_dialog.StartDialogLine != null)
            {
                _cursorManager.SetCursor(_cursorType);
            }
        }

        public override void SetPressableEffect()
        {
            if (_dialog.StartDialogLine != null)
            {
                _cursorManager.SetCursor(_cursorType, true, true);
            }
        }
    }
}