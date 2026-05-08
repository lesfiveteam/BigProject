using BigProject.Managers;
using BigProject.Systems.Inventory;
using UnityEngine;
using BigProject.Systems.QuestSystem;
using UnityEngine.Assertions;
using BigProject.Systems;
using BigProject.Player;

namespace BigProject.Gameplay.VillageChurchQuest
{
    public class QuestBoundariesController : MonoBehaviour, IQuestBoundariesController
    {
        [SerializeField]
        private GameObject _questObjects;
        [SerializeField]
        private QuestActions _questActions;
        [SerializeField]
        private GameObject _priest;
        [SerializeField]
        private Transform _churchDoorLeft, _churchDoorRight;
        [SerializeField]
        private Collider _doorCollider;
        [SerializeField]
        private float _churchDoorOpenAngleDelta;
        [SerializeField]
        private GameObject _chests;

        [field: SerializeField]
        public int QuestId { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Objects"));
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Actions"));
            Assert.IsNotNull(_priest, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Priest"));
            Assert.IsNotNull(_churchDoorLeft, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Door Left"));
            Assert.IsNotNull(_churchDoorRight, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Door Right"));
            Assert.IsNotNull(_doorCollider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Door Collider"));
            Assert.IsNotNull(_chests, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chests"));

        }

        public void InitOnSceneEntry()
        {
            _questObjects.SetActive(true);
            _chests.SetActive(true);
            _questActions.Init(ServiceLocator.GetService<InventorySystem>());
            _priest.SetActive(false);
            _doorCollider.enabled = true;
            Vector3 doorAngles = _churchDoorLeft.localEulerAngles;
            doorAngles.y += _churchDoorOpenAngleDelta;
            _churchDoorLeft.localEulerAngles = doorAngles;
            doorAngles = _churchDoorRight.localEulerAngles;
            doorAngles.y -= _churchDoorOpenAngleDelta;
            _churchDoorRight.localEulerAngles = doorAngles;
        }

        public void Begin()
        {
            InitOnSceneEntry();
        }
    }
}