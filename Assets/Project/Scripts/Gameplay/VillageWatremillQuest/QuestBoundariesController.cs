using BigProject.Managers;
using BigProject.Systems.Inventory;
using BigProject.Systems.HUD;
using UnityEngine;
using BigProject.Systems.QuestSystem;
using BigProject.Settings;

namespace BigProject.Gameplay.VillageWatermillQuest
{
    public class QuestBoundariesController : MonoBehaviour, IQuestBoundariesController
    {
        [SerializeField]
        private QuestActions _questActions;

        [SerializeField]
        private GameObject _miller;
        [SerializeField]
        private GameObject _elder;
        [SerializeField]
        private GameObject _questWatermillObjects;

        [field: SerializeField]
        public int QuestId { get; private set; }

        public void InitOnSceneEntry()
        {
            _questActions.Init(ServiceLocator.GetService<InventorySystem>(), ServiceLocator.GetService<RunesSystem>(),
                ServiceLocator.GetService<HUD>(), ServiceLocator.GetService<RuneShardsSystem>(), ServiceLocator.GetService<RunesConfig>());

            _miller.SetActive(true);
            _elder.SetActive(false);
            _questWatermillObjects.SetActive(true);
        }

        public void Begin()
        {
            InitOnSceneEntry();
        }
    }
}