using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Systems
{
    [Serializable]
    public struct Item
    {
        public string _name;
        public Sprite _itemSprite;
        public Sprite _noteSprite;
    }

    [CreateAssetMenu(menuName = "Inventory/ItemDatabase")]
    public class ItemsDatabaseSO : ScriptableObject
    {
        public List<Item> _items;
    }
}