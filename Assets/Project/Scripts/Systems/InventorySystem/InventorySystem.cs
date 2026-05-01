using BigProject.Managers;
using BigProject.Systems.Inventory.ItemsModifiers;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BigProject.Systems.Inventory
{
    public class InventorySystem : IDisposable, ISavable
    {
        private ItemsDatabaseSO _itemsDatabase;
        private ModifiersDatabaseSO _modifiersDatabase;
        private List<int> _heldItems;
        private Dictionary<string, List<ItemModifier>> _itemsModifiers = new();
        private DataToSave _dataToSave;
        public event Action OnInventoryUpdated;

        private const int INVENTORY_SIZE = 5;


        [Serializable]
        private class DataToSave
        {
            public List<int> items;
            public List<string> modifiers = new();
        }

        public string Key => "Inventory";

        public object SavingData
        {
            get
            {
                CreateDTO();
                return _dataToSave;
            }
        }

        public InventorySystem(ItemsDatabaseSO itemsDatabase, ModifiersDatabaseSO modifiersDatabase)
        {
            InitHeldItems();
            _itemsDatabase = itemsDatabase;
            _modifiersDatabase = modifiersDatabase;
            ExceptionUtilities.ThrowIfNull(_itemsDatabase, "InventorySystem", "itemsDatabase is null");
            ExceptionUtilities.ThrowIfNull(_modifiersDatabase, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "InventorySystem", "Modifiers Database"));
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        public void OnSaved(bool _) => _dataToSave = null;

        public void OnLoad()
        {
            if (_dataToSave == null)
            {
                return;
            }

            InitHeldItems();

            foreach (int id in _dataToSave.items)
            {
                AddToInventory(id);
            }

            _itemsModifiers.Clear();

            foreach (string modifierName in _dataToSave.modifiers)
            {
                AddItemModifier(modifierName);
            }

            _dataToSave = null;
            OnInventoryUpdated?.Invoke();
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void InitHeldItems()
        {
            _heldItems = new(INVENTORY_SIZE);

            for (int i = 0; i < INVENTORY_SIZE; i++)
            {
                _heldItems.Add(-1);
            }
        }

        private void CreateDTO()
        {
            if (_dataToSave == null)
            {
                _dataToSave = new();
            }

            _dataToSave.items = _heldItems;
            _dataToSave.modifiers.Clear();

            foreach (List<ItemModifier> itemModifiers in _itemsModifiers.Values)
            {
                foreach (ItemModifier itemModifier in itemModifiers)
                {
                    _dataToSave.modifiers.Add(itemModifier.ModifierName);
                }
            }
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

            GameLogManager.Info(String.Format(LogStr.INFO_SYSTEM, "InventorySystem", $"add item {value}"));
            OnInventoryUpdated?.Invoke();
        }
        
        //here, id is not a database id but an inventory id
        private void RemoveFromInventory(int id)
        {
            RemoveModifiers(id);

            for (int i = id; i < _heldItems.Count - 1; i++)
            {
                _heldItems[i] = _heldItems[i + 1];
            }

            _heldItems[_heldItems.Count - 1] = -1;

            GameLogManager.Info(String.Format(LogStr.INFO_SYSTEM, "InventorySystem", $"remove item {id}"));
            OnInventoryUpdated?.Invoke();
        }

        private void RemoveModifiers(int id) =>
            _itemsModifiers.Remove(_itemsDatabase._items.ElementAtOrDefault(_heldItems[id])._name);

        /// <summary>
        /// Adds item by its id in database
        /// </summary>
        public void AddItemByItemID(int itemID)
        {
            if (itemID >= _itemsDatabase._items.Count)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"itemID out of itemsDB bounds"));
                return;
            }

            AddToInventory(itemID);
        }

        public void AddItemByName(string itemName)
        {
            if (_itemsDatabase._items.Where(x => x._name.Equals(itemName)).Count() == 0)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"item {itemName} does not exist in itemsDB"));
                return;
            }
            
            int itemID = _itemsDatabase._items.IndexOf(_itemsDatabase._items.Where(x => x._name.Equals(itemName)).First());
            AddToInventory(itemID);
        }

        public void AddItemModifier(string itemModifierName)
        {
            if (!_modifiersDatabase.TryGetModifier(itemModifierName, out ItemModifier itemModifier))
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"has no modifier {itemModifierName}"));
                return;
            }

            string itemName = itemModifier.ItemName;

            if (!HasItemByName(itemName))
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "InventorySystem", $"has no item {itemName} to add modifier {itemModifierName}"));
                return;
            }

            int key = _heldItems.FindIndex(x => _itemsDatabase._items[x]._name.Equals(itemName));

            if (!_itemsModifiers.ContainsKey(itemName))
            {
                _itemsModifiers.Add(itemName, new());
            }
            else if (_itemsModifiers[itemName].Contains(itemModifier))
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "InventorySystem", $"already has modifier {itemModifierName} on item {itemName}"));
                return;
            }

            _itemsModifiers[itemName].Add(itemModifier);
            GameLogManager.Info(String.Format(LogStr.INFO_SYSTEM, "InventorySystem", $"add modifier \"{itemModifierName}\""));
            OnInventoryUpdated?.Invoke();
        }

        /// <summary>
        /// Removes item by its id in database
        /// </summary>
        public void RemoveItemById(int itemID)
        {
            if (_heldItems.Count == 0)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"can't remove item {itemID} from empty inventory"));
                return;
            }

            if (itemID >= _itemsDatabase._items.Count)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"item id {itemID} out of itemsDB bounds"));
                return;
            }

            int itemInventoryID = _heldItems.IndexOf(itemID);
            if (itemInventoryID == -1)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"item {itemID} does not exist in inventory"));
                return;
            }

            RemoveFromInventory(itemInventoryID);
        }

        public void RemoveItemByName(string itemName)
        {
            if (_heldItems.Count == 0)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"can't remove item {itemName} from empty inventory"));
                return;
            }

            if (_itemsDatabase._items.Where(x => x._name == itemName).Count() == 0)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"item {itemName} does not exist in itemsDB"));
                return;
            }

            int itemID = _itemsDatabase._items.IndexOf(_itemsDatabase._items.Where(x => x._name == itemName).First());
            int itemInventoryID = _heldItems.IndexOf(itemID);

            if (itemInventoryID == -1)
            {
                Debug.LogError(String.Format(LogStr.ERROR_SYSTEM, "InventorySystem", $"item {itemName} does not exist in inventory"));
                return;
            }

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
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if item exists by its database id
        /// </summary>
        public bool HasItemByID(int itemID)
        {
            if (_heldItems.Where((x) => x == itemID).Count() == 0)
            {
                return false;
            }

            return true;
        }

        /// <returns>All modifiers that item has.</returns>
        public IReadOnlyList<ItemModifier> GetHeldItemModifiers(string name) => _itemsModifiers.ContainsKey(name) ? _itemsModifiers[name] : null;

        /// <summary>
        /// Returns list of all held items
        /// </summary>
        public List<Item> GetAllHeldItems()
        {
            List<Item> items = new List<Item>();

            foreach (int id in _heldItems)
            {
                if (id == -1)
                {
                    break;
                }

                items.Add(_itemsDatabase._items[id]);
            }

            return items;
        }
    }
}