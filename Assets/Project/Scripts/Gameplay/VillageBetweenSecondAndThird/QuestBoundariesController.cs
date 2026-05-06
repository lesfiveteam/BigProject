using UnityEngine;
using BigProject.Systems.QuestSystem;
using UnityEngine.Assertions;
using BigProject.Systems;
using BigProject.Managers;
using BigProject.Player;
using UnityEngine.Localization;

namespace BigProject.Gameplay.VillageBetweenSecondAndThird
{
    public class QuestBoundariesController : MonoBehaviour, IQuestBoundariesController
    {
        [SerializeField]
        private GameObject _priest;
        [SerializeField]
        private Collider _churchDoor;
        [SerializeField]
        private float _churchDoorOpenAngleDelta = -15f;
        [SerializeField]
        private LocalizedString _playerRemark;

        private PlayerInputHandler _input;
        private GameplayManager _gameplayManager;


        [field: SerializeField]
        public int QuestId { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_churchDoor, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Church Door"));
            Assert.IsNotNull(_priest, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Priest"));
        }

        public void InitOnSceneEntry()
        {
            _priest.SetActive(false);
            _churchDoor.enabled = true;
            Vector3 doorAngles = _churchDoor.transform.localEulerAngles;
            doorAngles.y += _churchDoorOpenAngleDelta;
            _churchDoor.transform.localEulerAngles = doorAngles;
            _input = ServiceLocator.GetService<PlayerInputHandler>();
            _gameplayManager = ServiceLocator.GetService<GameplayManager>();
            _input.Click += OnClicked;
        }

        public void Begin()
        {
            InitOnSceneEntry();
        }

        private void OnClicked()
        {
            if (_gameplayManager.State == GameplayState.Play)
            {
                _input.Click -= OnClicked;
                ReplicaManager.ShowReplica(_playerRemark);
            }
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.Click -= OnClicked;
            }
        }
    }
}