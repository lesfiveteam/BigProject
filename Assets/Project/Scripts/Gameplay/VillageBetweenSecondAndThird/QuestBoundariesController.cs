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
        private Transform _churchDoorLeft, _churchDoorRight;
        [SerializeField]
        private Collider _doorCollider;
        [SerializeField]
        private float _churchDoorOpenAngleDelta;
        [SerializeField]
        private GameObject _chests;
        [SerializeField]
        private LocalizedString _playerRemark;

        private PlayerInputHandler _input;
        private GameplayManager _gameplayManager;


        [field: SerializeField]
        public int QuestId { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(_churchDoorLeft, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Door Left"));
            Assert.IsNotNull(_churchDoorRight, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Door Right"));
            Assert.IsNotNull(_doorCollider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Door Collider"));
            Assert.IsNotNull(_priest, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Priest"));
            Assert.IsNotNull(_chests, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chests"));
        }

        public void InitOnSceneEntry()
        {
            _priest.SetActive(false);
            _chests.SetActive(true);
            _doorCollider.enabled = true;
            Vector3 doorAngles = _churchDoorLeft.localEulerAngles;
            doorAngles.y += _churchDoorOpenAngleDelta;
            _churchDoorLeft.localEulerAngles = doorAngles;
            doorAngles = _churchDoorRight.localEulerAngles;
            doorAngles.y -= _churchDoorOpenAngleDelta;
            _churchDoorRight.localEulerAngles = doorAngles;
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