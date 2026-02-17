using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BigProject.Systems
{
    public class InventorySystem : IDisposable
    {
        private ItemsDatabaseSO _itemsDatabase;
        private List<int> _heldItems = new List<int>();
        public event Action OnInventoryUpdated;

        public InventorySystem(ItemsDatabaseSO itemsDatabase)
        {
            for (int i = 0; i < 5; i++)
            {
                _heldItems.Add(-1);
            }
            _itemsDatabase = itemsDatabase;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene _, Scene __)
        {
            OnInventoryUpdated?.Invoke();
        }

        private void AddToInventory(int value)
        {
            for (int i = 0; i < _heldItems.Count; i++)
            {
                if (_heldItems[i] == -1)
                {
                    _heldItems[i] = value;
                    break;
                }
            }

            OnInventoryUpdated?.Invoke();
        }
        
        //here, id is not a database id but an inventory id
        private void RemoveFromInventory(int id)
        {
            for (int i = id; i < _heldItems.Count - 1; i++)
            {
                _heldItems[i] = _heldItems[i + 1];
            }
            _heldItems[_heldItems.Count - 1] = -1;

            OnInventoryUpdated?.Invoke();
        }

        /// <summary>
        /// Adds item by its id in database
        /// </summary>
        public void AddItemByItemID(int itemID)
        {
            if (itemID >= _itemsDatabase._items.Count)
            {
                Debug.LogError($"Индекс выходит за границы БД предметов");
                return;
            }

            AddToInventory(itemID);
        }

        public void AddItemByName(string itemName)
        {
            if (_itemsDatabase._items.Where(x => x._name.Equals(itemName)).Count() == 0)
            {
                Debug.LogError($"Предмета {itemName} нет в БД предметов");
                return;
            }
            
            int itemID = _itemsDatabase._items.IndexOf(_itemsDatabase._items.Where(x => x._name.Equals(itemName)).First());
            AddToInventory(itemID);
        }

        /// <summary>
        /// Removes item by its id in database
        /// </summary>
        public void RemoveItemById(int itemID)
        {
            if (_heldItems.Count == 0)
            {
                Debug.LogError("Инвентарь пуст, невозможно удалить предмет");
                return;
            }

            if (itemID >= _itemsDatabase._items.Count)
            {
                Debug.LogError($"Индекс выходит за границы БД предметов");
                return;
            }

            int itemInventoryID = _heldItems.IndexOf(itemID);
            RemoveFromInventory(itemInventoryID);
        }

        public void RemoveItemByName(string itemName)
        {
            if (_heldItems.Count == 0)
            {
                Debug.LogError("Инвентарь пуст, невозможно удалить предмет");
                return;
            }

            if (_itemsDatabase._items.Where(x => x._name == itemName).Count() == 0)
            {
                Debug.LogError($"Предмета {itemName} нет в БД предметов");
                return;
            }

            int itemID = _itemsDatabase._items.IndexOf(_itemsDatabase._items.Where(x => x._name == itemName).First());
            if (itemID == -1)
            {
                Debug.LogError($"Предмета {itemName} нет в инвентаре");
                return;
            }

            int itemInventoryID = _heldItems.IndexOf(itemID);
            RemoveFromInventory(itemInventoryID);
        }

        /// <summary>
        /// Returns an item by its name. Use HasItemByName() beforehand
        /// </summary>
        public Item GetItemByName(string itemName)
        {
            return _itemsDatabase._items.Where(x => x._name == itemName).First();
        }

        /// <summary>
        /// Returns an item by its id in database. Use HasItemById() beforehand 
        /// </summary>
        public Item GetItemById(int itemID)
        {
            return _itemsDatabase._items[itemID];
        }

        public bool HasItemByName(string itemName)
        {
            if (_heldItems.Where((x) => x != -1 && _itemsDatabase._items[x]._name.Equals(itemName)).Count() == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if item exists by its database id
        /// </summary>
        public bool HasItemByID(int itemID)
        {
            if (_heldItems.Where((x) => x == itemID).Count() == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Returns list of all held items
        /// </summary>
        public List<Item> GetAllHeldItems()
        {
            List<Item> items = new List<Item>();
            foreach (int id in _heldItems)
            {
                if (id == -1)
                    break;
                items.Add(_itemsDatabase._items[id]);
            }
            return items;
        }
    }
}