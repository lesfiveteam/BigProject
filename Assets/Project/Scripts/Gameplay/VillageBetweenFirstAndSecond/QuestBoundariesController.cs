using UnityEngine;
using BigProject.Systems.QuestSystem;
using UnityEngine.Assertions;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using BigProject.Managers;
using BigProject.Managers.CutsceneManager;

namespace BigProject.Gameplay.VillageBetweenFirstAndSecond
{
    public class QuestBoundariesController : MonoBehaviour, IQuestBoundariesController
    {
        [SerializeField]
        private QuestActions _questActions;
        [SerializeField]
        private GameObject _questObjects;
        [SerializeField]
        private GameObject _miller;
        [SerializeField]
        private Transform _millerInsideMillPosition;
        [SerializeField]
        private QuestActionHandlerMono _enterVillageHandler;

        [field: SerializeField]
        public int QuestId { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "QuestActions"));
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Objects"));
            Assert.IsNotNull(_miller, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Miller"));
            Assert.IsNotNull(_millerInsideMillPosition, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Miller Inside Mill Position"));
            Assert.IsNotNull(_miller, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Enter Village Handler"));
        }

        public void InitOnSceneEntry()
        {
            _questActions.Init(ServiceLocator.GetService<InventorySystem>(), ServiceLocator.GetService<CutsceneManager>());
            _questObjects.SetActive(true);

            if (_enterVillageHandler.CurrentState == QuestActionState.Active)
            {
                _miller.transform.position = _millerInsideMillPosition.position;
            }
        }

        public void Begin()
        {
            _miller.SetActive(true);
            InitOnSceneEntry();
        }
    }
}