using BigProject.Managers;
using BigProject.Systems.Inventory;
using BigProject.Systems.HUD;
using UnityEngine;
using BigProject.Systems.QuestSystem;
using BigProject.Settings;

namespace BigProject.Gameplay.VillageChurchQuest
{
    public class QuestBoundariesController : MonoBehaviour, IQuestBoundariesController
    {
        [SerializeField]
        private GameObject _questObjects;
        [SerializeField]
        private QuestActions _questActions;

        [field: SerializeField]
        public int QuestId { get; private set; }

        public void InitOnSceneEntry()
        {
            _questObjects.SetActive(true);
            _questActions.Init(ServiceLocator.GetService<InventorySystem>());
        }

        public void Begin()
        {
            InitOnSceneEntry();
        }
    }
}