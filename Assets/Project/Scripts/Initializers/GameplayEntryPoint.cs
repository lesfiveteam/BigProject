using BigProject.Intercatable.HighlightedObjects;
using BigProject.Managers;
using BigProject.Managers.CursorManager;
using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Managers.CutsceneManager;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Systems.Inventory.ItemsModifiers;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using BigProject.UI.Common;
using BigProject.UI.Dialogue;
using BigProject.UI.Replica;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace BigProject.Initializers
{
    /// <summary>
    /// Services that persist between game scenes.
    /// </summary>
    public class GameplayEntryPoint : MonoBehaviour
    {
        [SerializeField]
        private HUDConfig _hudConfig;
        [SerializeField]
        private ItemsDatabaseSO _itemsDatabase;
        [SerializeField]
        private ModifiersDatabaseSO _modifiersDatabase;
        [SerializeField]
        private QuestJournalConfig _journalConfig;
        [SerializeField]
        private GameObject _hudPrefab;
        [SerializeField]
        private GameObject _dialogueView;
        [SerializeField]
        private GameObject _pauseView;
        [SerializeField]
        private GameObject _replicaView;
        [SerializeField]
        private QuestSwitchConfig _questSwitchConfig;
        [SerializeField]
        private QuestTrackerConfig _questTrackerConfig;
        [SerializeField]
        private PlayerController _playerControllerPrefab;
        [SerializeField]
        private GameObject _cursorManagerPrefab;
        [SerializeField]
        private CutscenesConfig _cutscenesConfig;

        [field: SerializeField]
        public Scenes _sceneToLoad; // For feature load progress

        private HUD _hud;
        private GameObject _hudObj;
        private GameObject _dialogueViewObj;
        private GameObject _replicaViewObj;
        private GameObject _pauseMenuViewObj;
        private QuestJournal _questJournal;
        private InventorySystem _inventory;
        private RunesSystem _runesSystem;
        private JournalUI _journalView;
        private InventoryUI _inventoryUI;
        private RunePanelUI _runeUI;
        private PlayerInputHandler _playerInput;
        private GameplayStatesHandler _statesHandler;
        private DialogueManager _dialogueManager;
        private ReplicaManager _replicaManager;
        private List<QuestSwitch> _questsSwitches = new();
        private QuestsBoundariesTracker _questsTracker;
        private PlayerSpawner _playerSpawner;
        private CutsceneManager _cutsceneManager;

        private static bool _isInstantiated;

        public static void Init()
        {
            _isInstantiated = false;
        }

        private void Awake()
        {
            if (_isInstantiated)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_DUPLICATE_UNIQUE_ENTITY, "Gameplay Entry Point"));
                Destroy(gameObject);
                return;
            }

            _isInstantiated = true;

            Assert.IsNotNull(_hudConfig, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "HUD config"));
            Assert.IsNotNull(_itemsDatabase, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "Items Database"));
            Assert.IsNotNull(_hudPrefab, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "HUD Prefab"));
            Assert.IsNotNull(_journalConfig, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "Journal Config"));
            Assert.IsNotNull(_questSwitchConfig, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "QuestSwitchConfig"));
            Assert.IsNotNull(_questTrackerConfig, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "QuestTrackerConfig"));
            Assert.IsNotNull(_playerControllerPrefab, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "PlayerController Prefab"));
            Assert.IsNotNull(_cursorManagerPrefab, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "CursorManager Prefab"));
            Assert.IsNotNull(_cutscenesConfig, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Gameplay Entry Point", "CutscenesConfig"));

            GameObject gameplayServices = new GameObject("GameplayServices");
            transform.parent = gameplayServices.transform; // For dispose after gameplay exit
            DontDestroyOnLoad(gameplayServices);
            InitServices();
        }

        public void InitServices()
        {
            GameLogManager.Info(LogStr.INFO_INITIALIZING_GAMEPLAY_SERVICES);
            ProgressManager progressManager = ServiceLocator.GetService<ProgressManager>();
            _inventory = new InventorySystem(_itemsDatabase, _modifiersDatabase);
            progressManager.AddSavable(_inventory);
            _hud = new();
            _playerInput = new();
            _questJournal = new QuestJournal(progressManager, _journalConfig);
            _runesSystem = new();
            GameplayManager gameplayManager = new(ServiceLocator.GetService<ManualLoop>());
            _statesHandler = new(_hudConfig, gameplayManager, _playerInput, _hud);
            _questsTracker = new(progressManager, _questTrackerConfig.QuestsIds.ToList());

            InitDialogue();
            InitReplica();
            InitPauseMenu();

            ServiceLocator.AddService(_questJournal);
            ServiceLocator.AddService(_runesSystem);
            ServiceLocator.AddService(_inventory);
            ServiceLocator.AddService(_hud);
            ServiceLocator.AddService(_playerInput);
            ServiceLocator.AddService(_dialogueManager);
            ServiceLocator.AddService(_replicaManager);
            ServiceLocator.AddService(gameplayManager);
            ServiceLocator.AddService(_questsTracker);

            InitHUD();
            _questJournal.Init();
            AddQuestsSwitches(progressManager);
            SceneLoadManager sceneLoader = ServiceLocator.GetService<SceneLoadManager>();
            CreatePlayer(sceneLoader);
            CreateCursorManager(sceneLoader);
            CreateCutsceneManager(sceneLoader);
            GameLogManager.Info(LogStr.INFO_INITIALIZING_GAMEPLAY_SERVICES_COMPLETED);
        }

        private void InitDialogue()
        {
            _dialogueViewObj = Instantiate(_dialogueView);
            _dialogueManager = new DialogueManager(_dialogueViewObj.GetComponent<DialogueUI>());
            _dialogueManager.Init();
            DontDestroyOnLoad(_dialogueViewObj);
        }

        private void InitReplica()
        {
            _replicaViewObj = Instantiate(_replicaView);
            _replicaManager = new ReplicaManager(_replicaViewObj.GetComponent<ReplicaView>());
            DontDestroyOnLoad(_replicaViewObj);
        }

        private void InitPauseMenu()
        {
            _pauseMenuViewObj = Instantiate(_pauseView);
            _pauseMenuViewObj.GetComponent<PauseMenuManager>().Init(_playerInput);
            DontDestroyOnLoad(_pauseMenuViewObj);
        }

        private void InitHUD()
        {
            GameLogManager.Info(LogStr.INFO_INITIALIZING_HUD);
            _hudObj = Instantiate(_hudPrefab);
            _journalView = _hudObj.GetComponentInChildren<JournalUI>();
            _runeUI = _hudObj.GetComponentInChildren<RunePanelUI>();
            _inventoryUI = _hudObj.GetComponentInChildren<InventoryUI>();
            CancelUI cancelUI = _hudObj.GetComponentInChildren<CancelUI>();
            ResetUI resetUI = _hudObj.GetComponentInChildren<ResetUI>();
            ServiceLocator.AddService(_inventoryUI);

            DontDestroyOnLoad(_hudObj);

            _journalView.Init(_questJournal);
            _inventoryUI.Init(_inventory);
            _runeUI.Init(_runesSystem);
            _hud = ServiceLocator.GetService<HUD>();
            _hud.AddWidget(_hudConfig.HUDInventoryWidgetId, _inventoryUI);
            _hud.AddWidget(_hudConfig.HUDJournalWidgetId, _journalView);
            _hud.AddWidget(_hudConfig.HUDRunesWidgetId, _runeUI);
            _hud.AddWidget(_hudConfig.HUDCancelWidgetId, cancelUI);
            _hud.AddWidget(_hudConfig.HUDResetWidgetId, resetUI);
            _hud.HideWidget(_hudConfig.HUDInventoryWidgetId);
            _hud.HideWidget(_hudConfig.HUDJournalWidgetId);
            _hud.HideWidget(_hudConfig.HUDCancelWidgetId);
            _hud.HideWidget(_hudConfig.HUDResetWidgetId);
            _hud.ShowWidget(_hudConfig.HUDInventoryWidgetId, 2f);
            _hud.ShowWidget(_hudConfig.HUDJournalWidgetId, 2f);
            GameLogManager.Info(LogStr.INFO_INITIALIZING_HUD_COMPLETED);
        }

        private void AddQuestsSwitches(ProgressManager progressManager)
        {
            foreach (QuestSwitchConfig.Condition switchCondition in _questSwitchConfig)
            {
                if (!progressManager.TryGetQuestActionHandler(switchCondition.QuestId, switchCondition.InitActionId, out IQuestActionHandler initActionHandler))
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "GameplayEntryPoint", "Unable to find init action for QuestSwitch"));
                    continue;
                }

                if (!progressManager.TryGetQuestActionHandler(switchCondition.TrackableQuestId, switchCondition.TrackableQuestActionId, 
                    out IQuestActionHandler trackableActionHandler))
                {
                    Debug.LogWarning(String.Format(LogStr.WARNING_SYSTEM, "GameplayEntryPoint", "Unable to find trackable action for QuestSwitch"));
                    continue;
                }

                QuestSwitch questSwitch = new(initActionHandler.Quest, trackableActionHandler.Quest, switchCondition.InitActionId, switchCondition.InitActionState);
                questSwitch.QuestSwitched += OnQuestSwitched;
                _questsSwitches.Add(questSwitch);
            }
        }

        private void OnQuestSwitched(QuestSwitch questSwitch)
        {
            questSwitch.QuestSwitched -= OnQuestSwitched;

            if (_questsSwitches.Remove(questSwitch))
            {
                questSwitch.Dispose();
            }
        }

        private void CreatePlayer(SceneLoadManager sceneLoader)
        {
            PlayerController playerController = Instantiate(_playerControllerPrefab);
            playerController.Init(_playerInput, sceneLoader);
            playerController.transform.parent = transform.parent;
            ServiceLocator.AddService(playerController);
            playerController.gameObject.SetActive(true);
            _playerSpawner = new(sceneLoader, playerController.GetComponent<NavMeshAgent>());
            ServiceLocator.AddService(_playerSpawner);

            // For case when run from gameplay scene.
            if (!string.Equals(Scenes.MainMenu.ToString(), SceneManager.GetActiveScene().name))
            {
                _playerSpawner.PositionPlayer(0);
            }
        }

        private void CreateCursorManager(SceneLoadManager sceneLoader)
        {
            GameObject cursorManagerObject = Instantiate(_cursorManagerPrefab, transform.parent);
            CursorManager cursorManager = cursorManagerObject.GetComponent<CursorManager>();
            ServiceLocator.AddService(cursorManager);
            InteractableObjectsHighlighter highlighter = cursorManagerObject.GetComponent<InteractableObjectsHighlighter>();

            if (highlighter == null)
            {
                Debug.LogWarning(String.Format(LogStr.ERROR_SYSTEM, "GameplayEntryPoint", "CursorManager has no highlighter"));
                return;
            }

            highlighter.Init(sceneLoader, cursorManager);
            cursorManagerObject.SetActive(true);

            // For case when run from gameplay scene.
            if (!string.Equals(Scenes.MainMenu.ToString(), SceneManager.GetActiveScene().name))
            {
                highlighter.RestartChecking();
            }
        }

        private void CreateCutsceneManager(SceneLoadManager sceneLoader)
        {
            GameObject cutsceneManagerObject = new("CutsceneManager");
            cutsceneManagerObject.transform.SetParent(transform.parent);
            PlayableDirector director = cutsceneManagerObject.AddComponent<PlayableDirector>();
            _cutsceneManager = new(director, sceneLoader, _cutscenesConfig);
            ServiceLocator.AddService(_cutsceneManager);

            // For case when run from gameplay scene.
            if (!string.Equals(Scenes.MainMenu.ToString(), SceneManager.GetActiveScene().name))
            {
                _cutsceneManager.FindActors();
            }
        }

        public void OnDestroy()
        {
            Remover.SafeDispose(_inventory);
            Remover.SafeDispose(_journalView);
            Remover.SafeDispose(_questJournal);
            Remover.SafeDispose(_hud);
            Remover.SafeDispose(_playerInput);
            Remover.SafeDispose(_statesHandler);

            Destroy(_hudObj);
            Destroy(_dialogueViewObj);
            Destroy(_replicaViewObj);
            Destroy(_pauseMenuViewObj);

            ServiceLocator.ReleaseService<QuestJournal>();
            ServiceLocator.ReleaseService<RunesSystem>();
            ServiceLocator.ReleaseService<InventorySystem>();
            ServiceLocator.ReleaseService<HUD>();
            ServiceLocator.ReleaseService<InventoryUI>();
            ServiceLocator.ReleaseService<PlayerInputHandler>();
            ServiceLocator.ReleaseService<GameplayManager>();
            ServiceLocator.ReleaseService<DialogueManager>();
            ServiceLocator.ReleaseService<ReplicaManager>();
            ServiceLocator.ReleaseService<PlayerController>();
            ServiceLocator.ReleaseService<CursorManager>();
            ServiceLocator.ReleaseService<PlayerSpawner>();
            ServiceLocator.ReleaseService<QuestsBoundariesTracker>();

            foreach (QuestSwitch questSwitch in _questsSwitches)
            {
                questSwitch.QuestSwitched -= OnQuestSwitched;
                questSwitch.Dispose();
            }

            _questsTracker?.Dispose();
            _playerSpawner?.Dispose();
            _cutsceneManager?.Dispose();
            Destroy(transform.parent.gameObject);
        }
    }
}