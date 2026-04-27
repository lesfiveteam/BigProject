using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers;
using BigProject.Player;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.VillageIntro
{
    public class QuestBoundariesController : MonoBehaviour, IQuestBoundariesController
    {
        [SerializeField]
        private QuestActions _questActions;
        [SerializeField]
        private GameObject _questObjects;

        [field: SerializeField]
        public int QuestId { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Actions"));
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Objects"));
        }

        public void InitOnSceneEntry()
        {
            _questActions.Init(ServiceLocator.GetService<PlayerController>(), ServiceLocator.GetService<GameplayManager>(), ServiceLocator.GetService<SceneLoadManager>());
            _questObjects.SetActive(true);
        }

        public void End()
        {
            _questObjects.SetActive(false);
        }
    }
}