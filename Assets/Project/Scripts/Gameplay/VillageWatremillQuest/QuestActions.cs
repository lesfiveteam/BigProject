using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Gameplay.Watermill;
using BigProject.Managers;
using BigProject.Managers.CutsceneManager;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using BigProject.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.Timeline;

namespace BigProject.Gameplay.VillageWatermillQuest
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private int _brokenLeverItemId;
        [SerializeField]
        private int _repairedLeverItemId;
        [SerializeField]
        private GameObject _miller;
        [SerializeField]
        private GameObject _chests;
        [SerializeField]
        private GameObject _runesInChest;
        [SerializeField]
        private GearsHandler _millWheelHandler;
        [SerializeField]
        private HUDConfig _hudConfig;
        [SerializeField]
        private int _questId = 2;
        [SerializeField]
        private Transform _runesInitialPoint;
        [SerializeField]
        private string _noteItemName;
        [SerializeField]
        private AssetReferenceT<TimelineAsset> _finishCutscene;
        [SerializeField]
        private QuestActionHandlerMono _cutsceneHandler;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _runeRemark;

        private InventorySystem _inventory;
        private RunesSystem _runes;
        private HUD _hud;
        private RunesConfig _runesConfig;
        private RunesDriver _runesDriver;
        private CutsceneManager _cutsceneManager;
        private SceneLoadManager _sceneLoader;
        private GameplayManager _gameplayManager;

        public void Init(InventorySystem inventory, RunesSystem runes, HUD hud, RuneShardsSystem runesSystem, RunesConfig runesConfig, 
            RunePanelUI runesPanel, CutsceneManager cutsceneManager, SceneLoadManager sceneLoader, GameplayManager gameplayManager)
        {
            _inventory = inventory;
            _runes = runes;
            _hud = hud;
            _runesConfig = runesConfig;
            _cutsceneManager = cutsceneManager;
            _sceneLoader = sceneLoader;
            _gameplayManager = gameplayManager;
            ExceptionUtilities.ThrowIfNull(_inventory, String.Format(gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_runes, String.Format(gameObject.name, "Rune System"));
            ExceptionUtilities.ThrowIfNull(_hud, String.Format(gameObject.name, "HUD"));
            ExceptionUtilities.ThrowIfNull(_runesConfig, String.Format(gameObject.name, "RuneShardsSystem"));
            ExceptionUtilities.ThrowIfNull(runesPanel, String.Format(gameObject.name, "RunePanelUI"));
            ExceptionUtilities.ThrowIfNull(_cutsceneManager, String.Format(gameObject.name, "CutsceneManager"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, String.Format(gameObject.name, "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(gameObject.name, "GameplayManager"));
            _runesDriver = new(runesSystem, runesConfig, runesPanel, _questId, _runesInitialPoint);
        }

        private void Awake()
        {
            Assert.IsNotNull(_miller, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Miller"));
            Assert.IsNotNull(_runesInChest, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Runes In Chest"));
            Assert.IsNotNull(_chests, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chests"));
            Assert.IsNotNull(_millWheelHandler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Wheel"));
            Assert.IsNotNull(_hudConfig, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "HUD confi"));
            Assert.IsNotNull(_runesInitialPoint, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "RunesInitialPoint"));
            Assert.IsNotNull(_finishCutscene, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Cutscene"));
            Assert.IsNotNull(_cutsceneHandler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Cutscene Handler"));
        }

        public void GetRepairedLever()
        {
            _inventory.RemoveItemById(_brokenLeverItemId);
            _inventory.AddItemByItemID(_repairedLeverItemId);
        }

        public void DespawnMiller()
        {
            //GameLogManager.Info(String.Format(LogStr.INFO_QUEST, "despawn miller from scene."));
            //_miller.SetActive(false);
        }

        public void SpawnMiller()
        {
            GameLogManager.Info(String.Format(LogStr.INFO_QUEST, "spawn chests."));
            _chests.SetActive(true);
            _runesInChest.SetActive(true);
        }

        public void RotateMillWheelOn()
        {
            GameLogManager.Info(String.Format(LogStr.INFO_QUEST, "Switch rotation of mill wheel on."));
            _millWheelHandler.enabled = true;
        }

        public void RemoveNote()
        {
            _inventory.RemoveItemByName(_noteItemName);
        }

        public void GetRune()
        {
            ReplicaManager.ShowReplica(_runeRemark);
            _runesDriver.Deliver(Camera.main);
            _runesInChest.SetActive(false);
        }

        public void PlayCutscene()
        {
            _gameplayManager.ChangeState(GameplayState.Cutscene);
            _cutsceneHandler.MakeTransition(0);
            StartCoroutine(PlayCutsceneRoutine());
        }

        private IEnumerator PlayCutsceneRoutine()
        {
            yield return new WaitUntil(() => !_sceneLoader.IsLoading);
            _cutsceneManager.Play(_finishCutscene, GameplayState.Cutscene, GameplayState.Play, false);
        }
    }
}