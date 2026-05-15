using Assets.Project.Scripts.Interactable;
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
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.TownHall
{
    public class EntryPoint : MonoBehaviour
    {
        private const float FADE_OUT_MUSIC_DURATION = 0.1f;
        private const float FADE_IN_MUSIC_DURATION = 0.1f;
        private const float IN_BUILD_MUSIC_VOLUME = 0.2f;

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
        [SerializeField]
        private CameraMove _cameraMove;
        [SerializeField]
        private AudioClip _villageMusic;

        private void Awake()
        {
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Actions"));
            Assert.IsNotNull(_chestPuzzle, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chest Puzzle"));
            Assert.IsNotNull(_miniGameActivator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Mini Game Activator"));
            Assert.IsNotNull(_townhallQuestObject, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest objects"));
            Assert.IsNotNull(_teleport, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "_teleport"));
            Assert.IsNotNull(_cameraMove, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "CameraMove"));
            Assert.IsNotNull(_villageMusic, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "_villageMusic"));
        }

        public void Init()
        {
            ProgressManager progressmanager = ServiceLocator.GetService<ProgressManager>();
            InventorySystem inventorySystem = ServiceLocator.GetService<InventorySystem>();
            InventoryUI inventoryUI = ServiceLocator.GetService<InventoryUI>();
            GameplayManager gameplayManager = ServiceLocator.GetService<GameplayManager>();
            PlayerInputHandler inputHandler = ServiceLocator.GetService<PlayerInputHandler>();
            PlayerController player = ServiceLocator.GetService<PlayerController>();
            MusicManager musicManager = ServiceLocator.GetService<MusicManager>();
            SoundsManager soundsManager = ServiceLocator.GetService<SoundsManager>();

            if (progressmanager.GetQuestState(_townhallQuestId) == Systems.QuestSystem.QuestState.Active)
            {
                _townhallQuestObject.SetActive(true);
            }

            musicManager.PlayMusic(_villageMusic, FADE_OUT_MUSIC_DURATION, FADE_IN_MUSIC_DURATION, IN_BUILD_MUSIC_VOLUME);

            _questActions.Init(
                inventorySystem, 
                inventoryUI, 
                gameplayManager, 
                ServiceLocator.GetService<RuneShardsSystem>(),
                ServiceLocator.GetService<RunesConfig>(), 
                ServiceLocator.GetService<RunePanelUI>());

            _chestPuzzle.Init(
                inventorySystem, 
                inventoryUI, 
                progressmanager, 
                ServiceLocator.GetService<HUD>(), 
                inputHandler, 
                soundsManager);

            _miniGameActivator.Init(
                gameplayManager, 
                inputHandler, 
                inventoryUI, 
                player.GetComponent<Collider>(),
                player.GetComponentInChildren<SkinnedMeshRenderer>());

            _teleport.Init(
                ServiceLocator.GetService<SceneLoadManager>(), 
                ServiceLocator.GetService<PlayerSpawner>(), 
                soundsManager);

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