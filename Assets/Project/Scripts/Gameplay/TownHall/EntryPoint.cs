using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Gameplay.Common;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.UI;
using BigProject.Utilities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.TownHall
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField]
        private QuestActions _questActions;
        [SerializeField]
        private ChestPuzzle _chestPuzzle;
        [SerializeField]
        private MiniGameActivator _miniGameActivator;
        [SerializeField]
        private GameObject _townhallQuestObject;
        [SerializeField]
        private int _townhallQuestId;
        [SerializeField]
        private TeleportHandler _teleport;

        private void Awake()
        {
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Actions"));
            Assert.IsNotNull(_chestPuzzle, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chest Puzzle"));
            Assert.IsNotNull(_miniGameActivator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Mini Game Activator"));
            Assert.IsNotNull(_townhallQuestObject, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest objects"));
        }

        public void Init()
        {
            ProgressManager progressmanager = ServiceLocator.GetService<ProgressManager>();

            if (progressmanager.GetQuestState(_townhallQuestId) == Systems.QuestSystem.QuestState.Active)
            {
                _townhallQuestObject.SetActive(true);
            }

            InventorySystem inventorySystem = ServiceLocator.GetService<InventorySystem>();
            InventoryUI inventoryUI = ServiceLocator.GetService<InventoryUI>();
            GameplayManager gameplayManager = ServiceLocator.GetService<GameplayManager>();
            PlayerInputHandler inputHandler = ServiceLocator.GetService<PlayerInputHandler>();
            PlayerController playerController = ServiceLocator.GetService<PlayerController>();
            SoundsManager soundsManager = ServiceLocator.GetService<SoundsManager>();

            _questActions.Init(inventorySystem, inventoryUI, gameplayManager, ServiceLocator.GetService<RunesSystem>());
            _chestPuzzle.Init(inventorySystem, inventoryUI, progressmanager, ServiceLocator.GetService<HUD>(), inputHandler, soundsManager);
            _miniGameActivator.Init(gameplayManager, inputHandler, inventoryUI, playerController.GetComponent<Collider>(),
                playerController.GetComponentInChildren<SkinnedMeshRenderer>());
            _teleport.Init(ServiceLocator.GetService<SceneLoadManager>(), ServiceLocator.GetService<PlayerSpawner>(), soundsManager);
        }
    }
}