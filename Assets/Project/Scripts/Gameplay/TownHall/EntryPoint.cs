using BigProject.Managers;
using BigProject.Systems;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.TownHall
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private QuestActions _questActions;
        [SerializeField]
        private ItemsDatabaseSO _itemsDB;

        private void Awake()
        {
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Actions"));
            Assert.IsNotNull(_itemsDB, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Items Database"));
        }

        public void Init()
        {
            _questActions.Init(ServiceLocator.GetService<InventorySystem>(), _itemsDB);
        }
    }
}