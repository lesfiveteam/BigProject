using Assets.Project.Scripts.Interactable;
using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Gameplay.Common;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using BigProject.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.Church
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private MiniGameActivator _miniGameActivator;
        [SerializeField]
        private BellsPuzzle _bellsPuzzle;
        [SerializeField]
        private TeleportHandler _teleport;
        [SerializeField]
        private CameraMove _cameraMove;
        [SerializeField]
        private int _questId;
        [SerializeField]
        private GameObject _questObjects;
        [SerializeField]
        private QuestActions _questActions;

        private void Awake()
        {
            Assert.IsNotNull(_miniGameActivator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Mini Game Activator"));
            Assert.IsNotNull(_bellsPuzzle, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Bells Puzzle"));
            Assert.IsNotNull(_cameraMove, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "CameraMove"));
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Game Objects"));
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Actions"));
        }

        public void Init()
        {
            PlayerInputHandler inputHandler = ServiceLocator.GetService<PlayerInputHandler>();
            GameplayManager gameplayManager = ServiceLocator.GetService<GameplayManager>();
            InventoryUI inventoryUI = ServiceLocator.GetService<InventoryUI>();
            PlayerController player = ServiceLocator.GetService<PlayerController>();
            SoundsManager soundsManager = ServiceLocator.GetService<SoundsManager>();
            ProgressManager progressManager = ServiceLocator.GetService<ProgressManager>();

            if (progressManager.GetQuestState(_questId) == Systems.QuestSystem.QuestState.Active)
            {
                _questObjects.SetActive(true);
            }

            _questActions.Init(ServiceLocator.GetService<InventorySystem>(), ServiceLocator.GetService<RuneShardsSystem>(), ServiceLocator.GetService<RunesConfig>());
            _miniGameActivator.Init(gameplayManager, inputHandler, inventoryUI, player.GetComponent<Collider>(), 
                player.GetComponentInChildren<SkinnedMeshRenderer>());
            _bellsPuzzle.Init(inputHandler, _miniGameActivator, soundsManager, progressManager);
            _teleport.Init(ServiceLocator.GetService<SceneLoadManager>(), ServiceLocator.GetService<PlayerSpawner>(), soundsManager);
            _cameraMove.Init(player);

            SwithOffOutline(player);
        }

        private void SwithOffOutline(PlayerController player)
        {
            Outline outline = player.GetComponentInChildren<Outline>();
            if (outline != null)
                outline.enabled = false;
        }
    }
}