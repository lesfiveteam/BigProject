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
        [SerializeField]
        private GameObject _miller;

        [field: SerializeField]
        public int QuestId { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Objects"));
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Miller"));
        }

        public void InitOnSceneEntry()
        {
            _questObjects.SetActive(true);
            _miller.SetActive(true);
        }

        public void Begin()
        {
            InitOnSceneEntry();
        }
    }
}