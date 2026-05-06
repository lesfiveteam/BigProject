using BigProject.Gameplay.Watermill;
using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;

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
        private Vector3 _millerFinalPosition;
        [SerializeField]
        private GearsHandler _millWheelHandler;
        [SerializeField]
        private HUDConfig _hudConfig;
        [SerializeField]
        private int _questId = 2;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _runeRemark;

        private InventorySystem _inventory;
        private RunesSystem _runes;
        private HUD _hud;
        private RuneShardsSystem _runesSystem;
        private RunesConfig _runesConfig;

        public void Init(InventorySystem inventory, RunesSystem runes, HUD hud, RuneShardsSystem runesSystem, RunesConfig runesConfig)
        {
            _inventory = inventory;
            _runes = runes;
            _hud = hud;
            _runesSystem = runesSystem;
            _runesConfig = runesConfig;
            ExceptionUtilities.ThrowIfNull(_inventory, String.Format(gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_runes, String.Format(gameObject.name, "Rune System"));
            ExceptionUtilities.ThrowIfNull(_hud, String.Format(gameObject.name, "HUD"));
        }

        private void Start()
        {
            Assert.IsNotNull(_miller, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Miller"));
            Assert.IsNotNull(_chests, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chests"));
            Assert.IsNotNull(_millWheelHandler, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Wheel"));
            Assert.IsNotNull(_hudConfig, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "HUD config"));
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
            GameLogManager.Info(String.Format(LogStr.INFO_QUEST, "move miller to quest final position and spawn chests."));
            _chests.SetActive(true);
            _miller.transform.position = _millerFinalPosition;
        }

        public void RotateMillWheelOn()
        {
            GameLogManager.Info(String.Format(LogStr.INFO_QUEST, "Switch rotation of mill wheel on."));
            _millWheelHandler.enabled = true;
        }

        public void GetRune()
        {
            IReadOnlyList<int> rewardRunes = _runesConfig.GetQuestRewardRunes(_questId);
            ExceptionUtilities.ThrowIfNullFormat(rewardRunes, "unable to get reward runes");

            foreach (int rewardRuneId in rewardRunes)
            {
                _runesSystem.AddRunesSegment(rewardRuneId);
            }

            ReplicaManager.ShowReplica(_runeRemark);
        }
    }
}