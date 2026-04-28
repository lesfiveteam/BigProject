using UnityEngine;
using BigProject.Systems.QuestSystem;
using UnityEngine.Assertions;
using BigProject.Systems;

namespace BigProject.Gameplay.VillageBetweenFirstAndSecond
{
    public class QuestBoundariesController : MonoBehaviour, IQuestBoundariesController
    {
        [SerializeField]
        private GameObject _questObjects;

        [field: SerializeField]
        public int QuestId { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Objects"));
        }

        public void InitOnSceneEntry()
        {
            _questObjects.SetActive(true);
        }

        public void Begin()
        {
            InitOnSceneEntry();
        }
    }
}