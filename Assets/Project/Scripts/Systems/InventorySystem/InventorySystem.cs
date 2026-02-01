using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BigProject.Systems
{
    [DefaultExecutionOrder(-1)]
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] private ItemsDatabaseSO _itemsDatabase;
        private List<int> _heldItems = new List<int>();

        public static InventorySystem Instance;
        public Action OnInventoryUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < 5; i++)
            { 
                _heldItems.Add(-1);
            }
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
        
        //здесь id - индекс элемента в инвентаре, не в БД
        private void RemoveFromInventory(int id)
        {
            if (id == _heldItems.Count - 1)
            {
                _heldItems[id] = -1;
            }
            else
            {
                for (int i = id; i < _heldItems.Count - 1; i++)
                {
                    _heldItems[i] = _heldItems[i + 1];
                }
                _heldItems[_heldItems.Count - 1] = -1;
            }

            OnInventoryUpdated?.Invoke();
        }

        /// <summary>
        /// Добалвяет предмет по его индексу в базе данных
        /// </summary>
        /// <param name="itemID">Индекс добавляемого элемента в базе данных</param>
        public void AddItemByItemID(int itemID)
        {
            if (itemID >= _itemsDatabase._items.Count)
            {
                Debug.LogError($"Индекс выходит за границы БД предметов");
                return;
            }

            AddToInventory(itemID);
        }

        /// <summary>
        /// Добавляет предмет по его имени
        /// </summary>
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
        /// Удаляет предмет по его индексу в БД предметов
        /// </summary>
        /// <param name="itemID">Индекс предмета в базе данных</param>
        public void RemoveItemById(int itemID)
        {
            if (itemID >= _itemsDatabase._items.Count)
            {
                Debug.LogError($"Индекс выходит за границы БД предметов");
                return;
            }

            int itemInventoryID = _heldItems.IndexOf(itemID);
            RemoveFromInventory(itemInventoryID);
        }

        /// <summary>
        /// Удаляет предмет по его имени
        /// </summary>
        public void RemoveItemByName(string itemName)
        {
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
        /// Возвращает предмет по его имени. Для проверки наличия предмета в инвентаре используйте HasItemByName()
        /// </summary>
        public Item GetItemByName(string itemName)
        {
            return _itemsDatabase._items.Where(x => x._name == itemName).First();
        }

        /// <summary>
        /// Возвращает предмет по его индексу в базе данных. Для проверки наличия предмета в инвентаре используйте HasItemById()
        /// </summary>
        public Item GetItemById(int itemID)
        {
            return _itemsDatabase._items[itemID];
        }

        /// <summary>
        /// Проверяет по имени предмета, что он существует в инвентаре
        /// </summary>
        public bool HasItemByName(string itemName)
        {
            if (_heldItems.Where((x) => x != -1 && _itemsDatabase._items[x]._name.Equals(itemName)).Count() == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Проверяет по индексу предмета в БД, что предмет существует в инвентаре
        /// </summary>
        public bool HasItemByID(int itemID)
        {
            if (_heldItems.Where((x) => x == itemID).Count() == 0)
                return false;

            return true;
        }

        /// <summary>
        /// Возвращает список предметов в инвентаре
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