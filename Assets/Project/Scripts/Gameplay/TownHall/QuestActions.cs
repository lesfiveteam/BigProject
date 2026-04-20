using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using BigProject.UI;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
namespace BigProject.Gameplay.TownHall
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private string _noteItemName;
        [SerializeField]
        private float _firstInteractionClueTime;
        [SerializeField]
        private string _brokenKeyItemName;
        [SerializeField]
        private GameObject _firstTouchChestTrigger;
        [SerializeField]
        private Collider _chestCollider;
        [SerializeField]
        private float _checkPillarsClueTime;
        [SerializeField]
        private float _checkTownhallClueTime;
        [SerializeField]
        private GameObject _rune;
        [SerializeField]
        private int _questId = 1;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _firstEnterRemark;
        [SerializeField]
        private LocalizedString _brokenKeyRemark;
        [SerializeField]
        private LocalizedString _examineRoomRemark;
        [SerializeField]
        private LocalizedString _examinePillarsRemark;
        [SerializeField]
        private LocalizedString _needFatherRemark;
        [SerializeField]
        private LocalizedString _openChestRemark;
        [SerializeField]
        private LocalizedString _getKeyRemark;

        private InventorySystem _inventory;
        private InventoryUI _inventoryUI;
        private Coroutine _firstInteractionClue;
        private Coroutine _interactPillarsClue;
        private GameplayManager _gameplayManager;
        private RuneShardsSystem _runesSystem;
        private RunesConfig _runesConfig;

        public void Init(InventorySystem inventory, InventoryUI inventoryUI, GameplayManager gameplayManager, RuneShardsSystem runesSystem, RunesConfig runesConfig)
        {
            _inventory = inventory;
            _inventoryUI = inventoryUI;
            _gameplayManager = gameplayManager;
            _runesSystem = runesSystem;
            _runesConfig = runesConfig;
            ExceptionUtilities.ThrowIfNull(_inventory, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_inventoryUI, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory UI"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Gameplay Manager"));
            ExceptionUtilities.ThrowIfNull(_runesSystem, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "RuneShardsSystem"));
            ExceptionUtilities.ThrowIfNull(_runesConfig, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "RunesConfig"));
        }

        private void Awake()
        {
            Assert.IsNotNull(_firstTouchChestTrigger, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "First touch trigger"));
            Assert.IsNotNull(_chestCollider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chest collider"));
            Assert.IsNotNull(_rune, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Rune"));
        }

        // Draw clue to note texture.
        public void AddRecordToNote(string recordName)
        {
            if (!_inventory.HasItemByName(_noteItemName))
            {
                GameLogManager.Info(String.Format(LogStr.INFO_QUEST, "add townhall empty note"));
                _inventory.AddItemByName(_noteItemName);
                _inventoryUI.SetNoteVisibility(true);
            }

            GameLogManager.Info(String.Format(LogStr.INFO_QUEST, $"add townhall note record {recordName}"));
            _inventory.AddItemModifier(recordName);
        }

        public void AllRecordsCollected()
        {
            ReplicaManager.ShowReplica(_needFatherRemark);// "Узоры схожи с бородками на ключах. Такие только отец в кузне сможет выточить...");
        }

        public void FirstTownHallMeeting()
        {
            ReplicaManager.ShowReplica(_firstEnterRemark);// "Время не пощадило это место. Интересно, что за сундук виднеется в конце зала?");
        }

        public void FirstInteraction(bool isCompleted)
        {
            if (isCompleted && _firstTouchChestTrigger != null)
            {
                // ReplicaManager.ShowReplica("Теперь сундук");
                Destroy(_firstTouchChestTrigger);
            }

            //if (isCompleted)
            //{
            //    //if (_firstInteractionClue != null)
            //    //{
            //    //    StopCoroutine(_firstInteractionClue);
            //    //}

         //   }
         //   }
            //else
            //{
            //    _firstInteractionClue = StartCoroutine(InteractionClueRoutune());
            //}
        }

        public void GetBrokenKey()
        {
            GameLogManager.Info(String.Format(LogStr.INFO_QUEST, "add broken key"));
            ReplicaManager.ShowReplica(_getKeyRemark);
            _inventory.AddItemByName(_brokenKeyItemName);
        }

        public void FirstTouchChest()
        {
            ReplicaManager.ShowReplica(_brokenKeyRemark);// "Быть может, один из этой кучи ключей подойдёт к замку?");
        }

        public void ReadyToInsertKey()
        {
            if (_firstTouchChestTrigger != null)
            {
                _firstTouchChestTrigger.SetActive(false);
            }

            _chestCollider.enabled = true;
        }

        public void BrokenKeyInserted()
        {
            StartCoroutine(GameplayUtilities.DoAfterConditionRoutine(() => _gameplayManager.State == GameplayState.Play, 
            () =>
            {
                _chestCollider.enabled = false;
                ReplicaManager.ShowReplica(_examineRoomRemark);// "Всё бестолку! Надо осмотреться, может удастся найти что-то интересное...");
            }
            ));
        }

        public void InteractPillarsClue(bool isActive)
        {
            if (isActive)
            {
                _interactPillarsClue = StartCoroutine(CheckPillarsRoutine());
            }
            else if (_interactPillarsClue != null)
            {
                StopCoroutine(_interactPillarsClue);
            }
        }

        public void PuzzleWasPlayed()
        {
            StartCoroutine(GameplayUtilities.DoAfterConditionRoutine(() => _gameplayManager.State == GameplayState.Play,
                () =>
                {
                    _chestCollider.enabled = false;
                    ReplicaManager.ShowReplica(_openChestRemark);// "Да! Получилось! Что же скрывал этот сундук за такими хитрыми замками?");
                }
                ));
        }

        public void ShowRune()
        {
            _rune.SetActive(true);
        }

        public void GetRune()
        {
            IReadOnlyList<int> rewardRunes = _runesConfig.GetQuestRewardRunes(_questId);
            ExceptionUtilities.ThrowIfNullFormat(rewardRunes, "unable to get reward runes");

            foreach (int rewardRuneId in rewardRunes)
            {
                _runesSystem.AddRunesSegment(rewardRuneId);
            }
        }

        //private IEnumerator InteractionClueRoutune()
        //{
        //    yield return new WaitForSeconds(_firstInteractionClueTime);
        //    //ReplicaManager.ShowReplica("Надо подергать что-то");
        //}

        private IEnumerator CheckPillarsRoutine()
        {
            yield return new WaitForSeconds(_checkPillarsClueTime);
            ReplicaManager.ShowReplica(_examinePillarsRemark);// "На колоннах видны странные символы, стоит посмотреть поближе...");
        }
    }
}
