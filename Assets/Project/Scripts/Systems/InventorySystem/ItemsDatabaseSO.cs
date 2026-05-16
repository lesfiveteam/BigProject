using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems.Inventory
{
    [Serializable]
    public struct Item
    {
        /// <summary>
        /// name in English for internal work
        /// </summary>
        public string _name;
        /// <summary>
        /// name in Russian to be displayed in UI elements
        /// </summary>
        public string _nameLocalized;

        [field: SerializeField]
        public string VerbToGet { private set; get; }

        [field: SerializeField]
        public string VerbToGive { private set; get; }

        public Sprite _itemSprite;
        public Sprite _noteSprite;
        public bool _isAddedAtMiniGame;
    }

    [CreateAssetMenu(menuName = "Inventory/ItemDatabase")]
    public class ItemsDatabaseSO : ScriptableObject
    {
        public List<Item> _items;
    }
}