using Assets.Project.Scripts.Interactable;
using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Gameplay.Common;
using BigProject.Managers;
using BigProject.Managers.CursorManager;
using BigProject.Managers.CutsceneManager;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Gameplay.Church
{
    public class EntryPoint : MonoBehaviour
    {
        private const float FADE_OUT_MUSIC_DURATION = 0.1f;
        private const float FADE_IN_MUSIC_DURATION = 0.1f;
        private const float IN_BUILD_MUSIC_VOLUME = 0.2f;

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
        private int _finalQuestId;
        [SerializeField]
        private GameObject _questObjects;
        [SerializeField]
        private QuestActions _questActions;
        [SerializeField]
        private Final.QuestActions _finalQuestActions;
        [SerializeField]
        private AudioClip _villageMusic;

        private ProgressManager _progressManager;

        private void Awake()
        {
            Assert.IsNotNull(_miniGameActivator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Mini Game Activator"));
            Assert.IsNotNull(_bellsPuzzle, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Bells Puzzle"));
            Assert.IsNotNull(_cameraMove, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "CameraMove"));
            Assert.IsNotNull(_questObjects, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Game Objects"));
            Assert.IsNotNull(_questActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest Actions"));
            Assert.IsNotNull(_finalQuestActions, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Final Quest Actions"));
            Assert.IsNotNull(_villageMusic, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "_villageMusic"));
        }

        public void Init()
        {
            PlayerInputHandler inputHandler = ServiceLocator.GetService<PlayerInputHandler>();
            GameplayManager gameplayManager = ServiceLocator.GetService<GameplayManager>();
            InventoryUI inventoryUI = ServiceLocator.GetService<InventoryUI>();
            PlayerController player = ServiceLocator.GetService<PlayerController>();
            SoundsManager soundsManager = ServiceLocator.GetService<SoundsManager>();
            MusicManager musicManager = ServiceLocator.GetService<MusicManager>();
            _progressManager = ServiceLocator.GetService<ProgressManager>();

            musicManager.PlayMusic(_villageMusic, FADE_OUT_MUSIC_DURATION, FADE_IN_MUSIC_DURATION, IN_BUILD_MUSIC_VOLUME);

            if (_progressManager.GetQuestState(_questId) == Systems.QuestSystem.QuestState.Active)
            {
                _questObjects.SetActive(true);
            }
            else
            {
                _progressManager.AddQuestListener(_questId, OnQuestStateChanged);
            }

            SkinnedMeshRenderer playerRenderer = player.GetComponentInChildren<SkinnedMeshRenderer>();
            Collider playerCollider = player.GetComponent<Collider>();

            _questActions.Init(ServiceLocator.GetService<InventorySystem>(), ServiceLocator.GetService<RuneShardsSystem>(), 
                ServiceLocator.GetService<RunesConfig>(), ServiceLocator.GetService<RunePanelUI>(), playerCollider, playerRenderer,
                gameplayManager, ServiceLocator.GetService<HUD>(), inventoryUI);
            _miniGameActivator.Init(gameplayManager, inputHandler, inventoryUI, playerCollider, playerRenderer);
            _bellsPuzzle.Init(inputHandler, _miniGameActivator, soundsManager, _progressManager, ServiceLocator.GetService<CutsceneManager>());
            _teleport.Init(ServiceLocator.GetService<SceneLoadManager>(), ServiceLocator.GetService<PlayerSpawner>(), soundsManager);
            _cameraMove.Init(player);

            SwithOffOutline(player);

            _finalQuestActions.Init(ServiceLocator.GetService<ProgressManager>(), ServiceLocator.GetService<RunePanelUI>(),
                ServiceLocator.GetService<CutsceneManager>(), ServiceLocator.GetService<GameplayManager>(), ServiceLocator.GetService<CursorManager>(),
                ServiceLocator.GetService<DialogueManager>(), ServiceLocator.GetService<SceneLoadManager>(), ServiceLocator.GetService<MusicManager>(), playerRenderer);
            _progressManager.AddQuestListener(_finalQuestId, OnQuestStateChanged);

            if (_progressManager.GetQuestState(_finalQuestId) == QuestState.Active)
            {
                _finalQuestActions.gameObject.SetActive(true);
                _finalQuestActions.OnRunesAssembled();
            }
        }

        private void OnQuestStateChanged(IQuest quest)
        {
            if (quest.CurrentState != QuestState.Active)
            {
                return;
            }

            if (quest.ID == _questId)
            {
                _questObjects.SetActive(true);
            }
            else
            {
                _finalQuestActions.gameObject.SetActive(true);
                _finalQuestActions.OnRunesAssembled();
            }
        }

        private void SwithOffOutline(PlayerController player)
        {
            Outline outline = player.GetComponentInChildren<Outline>();
            if (outline != null)
                outline.enabled = false;
        }

        private void OnDestroy()
        {
            _progressManager.RemoveQuestListener(_questId, OnQuestStateChanged);
            _progressManager.RemoveQuestListener(_finalQuestId, OnQuestStateChanged);
        }
    }
}