using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.Inventory;
using BigProject.UI;
using BigProject.Utilities;
using Managers.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;

namespace BigProject.Gameplay.VillageElderQuest
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private string _bagItemName;
        [SerializeField]
        private GameObject _bag;
        [SerializeField]
        private GameObject _delegation;
        [SerializeField]
        private AmbassadorDialogueManager _ambassador;
        [SerializeField]
        private Transform _delegationNearElderPosition;
        [SerializeField]
        private List<string> _keysItemsNames;
        [SerializeField]
        private GameObject _enterVillageTrigger;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _townhallRemark;
        [SerializeField]
        private LocalizedString _needFatherRemark;
        [SerializeField]
        private LocalizedString _watermillRemark;

        private InventorySystem _inventory;
        private InventoryUI _inventoryUI;
        private GameplayManager _gameplayManager;

        public void Init(InventorySystem inventory, InventoryUI inventoryUI, GameplayManager gameplayManager)
        {
            _inventory = inventory;
            _inventoryUI = inventoryUI;
            _gameplayManager = gameplayManager;
            ExceptionUtilities.ThrowIfNull(_inventory, String.Format(gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_inventoryUI, String.Format(gameObject.name, "Inventory UI"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, String.Format(gameObject.name, "Gameplay Manager"));
        }

        private void Awake()
        {
            Assert.IsNotNull(_bag, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Bag"));
            Assert.IsNotNull(_delegation, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Delegation"));
            Assert.IsNotNull(_ambassador, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Ambassador"));
            Assert.IsNotNull(_delegationNearElderPosition, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Delegation Near Elder Position"));
        }

        public void ShowBag()
        {
            _bag.SetActive(true);
        }

        public void GetBag()
        {
            _inventory.AddItemByName(_bagItemName);
            ReplicaManager.ShowReplica(_townhallRemark, 1f);
        }

        public void RemoveBag()
        {
            RemoveAmbassador();
            _inventory.RemoveItemByName(_bagItemName);
            GameplayUtilities.DoAfterConditionRoutine(() => _gameplayManager.State == GameplayState.Play, () =>
            ReplicaManager.ShowReplica(_needFatherRemark, 1f));
        }

        public void AmbassadorAppearance()
        {
            _inventoryUI.SetNoteVisibility(false);
            _delegation.SetActive(true);
        }

        public void AmbassadorNearElder()
        {
            _delegation.SetActive(true);
            _delegation.transform.position = _delegationNearElderPosition.position;
            _delegation.transform.rotation = _delegationNearElderPosition.rotation;
            _ambassador.MoveToElder();
        }

        public void RemoveAmbassador()
        {
            if (_ambassador != null)
            {
                Destroy(_ambassador);
            }
        }

        public void GetKeys()
        {
            foreach (string key in _keysItemsNames)
            {
                _inventory.AddItemByName(key);
            }
        }

        public void EnterVillage()
        {
            _enterVillageTrigger.SetActive(true);
        }

        public void CheckWatermill()
        {
            ReplicaManager.ShowReplica(_watermillRemark);
        }
    }
}
