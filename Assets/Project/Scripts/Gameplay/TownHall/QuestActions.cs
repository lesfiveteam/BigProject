using BigProject.Managers;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.TownHall
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private int _pillarNoteId;
        [SerializeField]
        private PillarNote[] _pillarsNotes;

        private InventorySystem _inventory;
        private ItemsDatabaseSO _itemsDB;
        private Texture2D _pillarNoteTexInitial;
        private Texture2D _pillarNoteTex;

        [Serializable]
        private struct PillarNote
        {
            public Texture2D image;
            public Vector2 uv;
        }

        public void Init(InventorySystem inventory, ItemsDatabaseSO itemsDB)
        {
            _inventory = inventory;
            _itemsDB = itemsDB;
            ExceptionUtilities.ThrowIfNull(_inventory, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_itemsDB, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Items Database"));
        }

        private void Start()
        {
            Item pillarNote = _itemsDB._items.ElementAtOrDefault(_pillarNoteId);
            ExceptionUtilities.ThrowIfNull(pillarNote, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Piallar Note"));
            _pillarNoteTex = pillarNote._noteSprite.texture;
            ExceptionUtilities.ThrowIfNull(_pillarNoteTex, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Piallar Note Texture"));
            Assert.IsNotNull(_pillarsNotes, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject, "Pillars Notes"));
            _pillarNoteTexInitial = Instantiate(_pillarNoteTex);
        }

        public void AddPillarNote(int pillarId)
        {
            if (pillarId >= _pillarsNotes.Length)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_QUEST, $"pillar id {pillarId} out of notes range"));
                return;
            }

            if (!_inventory.HasItemByID(_pillarNoteId))
            {
                _inventory.AddItemByItemID(_pillarNoteId);
            }

            GameLogManager.Info(String.Format(LogStr.INFO_QUEST, $"get pillar {pillarId} note"));
            PillarNote pillarNote = _pillarsNotes.ElementAtOrDefault(pillarId);

            if (pillarNote.image == null)
            {
                return;
            }

            Vector2Int texPos = new((int)(_pillarNoteTex.width * pillarNote.uv.x), (int)(_pillarNoteTex.height * pillarNote.uv.y));

            try
            {
                _pillarNoteTex.SetPixels(texPos.x, texPos.y, pillarNote.image.width, pillarNote.image.height, pillarNote.image.GetPixels());
            }
            catch (Exception e)
            {
                Debug.LogError(String.Format(LogStr.ERROR_QUEST, $"unable to write pillar note to texture. {e.Message}"));
            }

            _pillarNoteTex.Apply();
        }

        private void OnDestroy()
        {
            _pillarNoteTex.SetPixels(_pillarNoteTexInitial.GetPixels());
            _pillarNoteTex.Apply();
        }
    }
}
