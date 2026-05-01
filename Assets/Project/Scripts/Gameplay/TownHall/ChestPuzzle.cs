using BigProject.Gameplay.Common;
using BigProject.Intercatable;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Player;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using BigProject.Utilities;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;

namespace BigProject.Gameplay.TownHall
{
    public class ChestPuzzle : MonoBehaviour, IInteractable, ISavable
    {
        [SerializeField]
        private MiniGameActivator _activator;
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private List<Collider> _keysHolders;
        [SerializeField]
        private List<GameObject> _keysPrefabs;
        [SerializeField]
        private List<int> _correctKeysIds;
        [SerializeField]
        private Transform _chestCup;
        [SerializeField]
        private string _brokenKeyItemName;
        [SerializeField]
        private float _brokenKeyMovingTime;
        [SerializeField]
        private float _keysMovingTime;
        [SerializeField]
        private float _openChestMovingTime;
        [SerializeField]
        private QuestActionHandlersContainer _actionHandlers;
        [SerializeField]
        private string _actionTryBrokenKeyName;
        [SerializeField]
        private string _actionOpenName;
        [SerializeField]
        private string _noteItemName;
        [SerializeField]
        private float _timeBeforeClue;
        [SerializeField]
        private HUDConfig _hudConfig;
        [SerializeField]
        private AudioClip _chestOpenSound;
        [SerializeField]
        private float _chestOpenSoundVolume = 1f;
        [SerializeField]
        private Collider _maskCollider;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _puzzleRemark;
        [SerializeField]
        private LocalizedString _incorrectKeysRemark;

        private InventorySystem _inventory;
        private InventoryUI _inventoryUI;
        private List<int> _keysIds;
        private List<GameObject> _keys = new();
        private List<string> _keysNames = new();
        private DataToSave _dataToSave = new();
        private ProgressManager _progressManager;
        private bool _returnToInventory = true;
        private HUD _hud;
        private PlayerInputHandler _inputHandler;
        private Coroutine _clueCoroutine;
        private SoundsManager _soundsManager;

        private readonly Vector3 CHEST_CUP_OPEN_ROTATION_OFFSET = new(110f, 0f, 0f);
        private readonly Vector3 KEYS_ROTATION_OFFSET = new(-180f, 0f, 0f);


        [Serializable]
        private class DataToSave
        {
            [Serializable]
            public class HolderKeyPair
            {
                public string keyName;
                public int holderId;
                public int keyId;
            }

            public List<HolderKeyPair> keysData = new();
        }

        public string Key => "Townhall_data";

        public object SavingData => _dataToSave;

        public Vector3 TargetPosition => _target.position;

        [field: SerializeField]
        public float MaxDistance { get; private set; }

        public bool NeedLookAt => true;

        public Vector3 TargetLookAt => transform.position;

        public void Init(InventorySystem inventory, InventoryUI inventoryUI, ProgressManager progressManager, HUD hud, PlayerInputHandler inputHandler, SoundsManager soundsManager)
        {
            _inventory = inventory;
            _inventoryUI = inventoryUI;
            _progressManager = progressManager;
            _hud = hud;
            _inputHandler = inputHandler;
            _soundsManager = soundsManager;
            ExceptionUtilities.ThrowIfNull(_inventory, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Gameplay Manager"));
            ExceptionUtilities.ThrowIfNull(_inventoryUI, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Input Handler"));
            ExceptionUtilities.ThrowIfNull(_progressManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Progress Manager"));
            ExceptionUtilities.ThrowIfNull(_hud, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "HUD"));
            ExceptionUtilities.ThrowIfNull(_inputHandler, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Input Handler"));
            ExceptionUtilities.ThrowIfNull(_puzzleRemark, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "LocalizedString puzzle remark"));
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Sounds manager"));
        }

        private void Awake()
        {
            Assert.IsNotNull(_activator, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Activator"));
            Assert.IsNotNull(_target, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Target position"));
            Assert.IsNotNull(_chestCup, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Chest cup"));
            Assert.IsNotNull(_actionHandlers, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Quest action handler"));
            Assert.IsNotNull(_hudConfig, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "HUD Config"));
            Assert.IsNotNull(_puzzleRemark, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Puzzle Remark"));
            Assert.IsNotNull(_incorrectKeysRemark, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Incorrect Keys Remark"));
            Assert.IsNotNull(_maskCollider, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Mask Collider"));
            _keysIds = Enumerable.Repeat(-1, _keysHolders.Count).ToList();
        }

        private void Start()
        {
            _progressManager.LoadAdditionalData(this);
        }

        public void OnLoad()
        {
            foreach (DataToSave.HolderKeyPair holderKeyPair in _dataToSave.keysData)
            {
                SetKeyToHolder(holderKeyPair.keyName, holderKeyPair.holderId, holderKeyPair.keyId);
            }

            MoveElement(_chestCup, Vector3.zero, CHEST_CUP_OPEN_ROTATION_OFFSET, 0f);
        }

        public void Interact()
        {
            _activator.ActivateMiniGame();

            if (_actionHandlers[_actionOpenName].CurrentState == QuestActionState.Active)
            {
                _hud.ShowWidget(_hudConfig.HUDResetWidgetId);

                if (_keys.Count == 0)
                {
                    if (_clueCoroutine != null)
                    {
                        StopCoroutine(_clueCoroutine);
                    }

                    _clueCoroutine = StartCoroutine(ClueRoutine());
                }
            }
        }

        public void InstallKey(string itemName, int keyHolderId, int keyPrefabId)
        {
            if (!SetKeyToHolder(itemName, keyHolderId, keyPrefabId))
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_QUEST, "chest puzzle unable to set key to holder"));
                return;
            }

            if (_inventory.HasItemByName(itemName))
            {
                _inventory.RemoveItemByName(itemName);
            }

            if (itemName.Equals(_brokenKeyItemName))
            {
                RemoveBrokenKey();
                return;
            }

            _dataToSave.keysData.Add(new DataToSave.HolderKeyPair() 
            {
                keyName = itemName,
                holderId = keyHolderId, 
                keyId = keyPrefabId 
            });

            if (_clueCoroutine != null)
            {
                StopCoroutine(_clueCoroutine);
            }

            if (IsAllKeysInside())
            {
                ApplyKeys();
            }
        }

        private void RemoveBrokenKey()
        {
            _keysNames.Clear();
            GameObject key = _keys.ElementAtOrDefault(0);
            _keys.Clear();
            ResetKeysIds();
            
            if (key != null)
            {
                StartCoroutine(RemoveBrokenKeyRoutine(key.transform));
            }
        }

        private IEnumerator RemoveBrokenKeyRoutine(Transform key)
        {
            //Transform partOne = key.GetChild(0);
            //Transform partTwo = key.GetChild(1);
            Animator animator = key.GetComponentInChildren<Animator>();
            ExceptionUtilities.ThrowIfNullFormat(animator, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Broken key Animator"));

            //if (partOne == null || partTwo == null)
            //{
            //    Debug.LogError(String.Format(LogStr.ERROR_QUEST, "unable to get broken key part"));
            //    yield break;
            //}

            animator.SetTrigger("Broken");

            //MoveElement(partOne, BROKEN_KEY_PART_1_MOVING_OFFSET, BROKEN_KEY_PART_1_ROTATION_OFFSET, _brokenKeyMovingTime);
            //MoveElement(partTwo, BROKEN_KEY_PART_2_MOVING_OFFSET, BROKEN_KEY_PART_2_ROTATION_OFFSET, _brokenKeyMovingTime);
            _actionHandlers[_actionTryBrokenKeyName].MakeTransition(0);
            yield return new WaitForSeconds(_brokenKeyMovingTime + 0.1f);
            Destroy(key.gameObject);
            _activator.DeactivateMiniGame();
        }

        private void MoveElement(Transform key, Vector3 positionOffset, Vector3 anglesOffset, float time)
        {
            Vector3 newPosition = key.localPosition;
            Vector3 newAngles = key.localEulerAngles;
            newPosition += positionOffset;
            newAngles += anglesOffset;
            key.DOLocalRotate(newAngles, time);
            key.DOLocalMove(newPosition, time);
        }

        private bool SetKeyToHolder(string itemName, int keyHolderId, int keyPrefabId)
        {
            Collider keyCollider = _keysHolders.ElementAtOrDefault(keyHolderId);
            Transform keyHolder = keyCollider != null ? keyCollider.transform : null;

            if (keyHolder == null)
            {
                Debug.LogError(string.Format(LogStr.ERROR_QUEST, $"chest puzzle unable to use key holder {keyHolderId}"));
                return false;
            }

            if (_keysIds[keyHolderId] >= 0)
            {
                GameLogManager.Info(string.Format(LogStr.INFO_QUEST, $"chest puzzle try to fill busy key holder {keyHolderId}"));
                return false;
            }

            GameObject keyPrefab = _keysPrefabs.ElementAtOrDefault(keyPrefabId);

            if (keyPrefab == null)
            {
                Debug.LogError(string.Format(LogStr.ERROR_QUEST, $"chest puzzle unable to instantiate key prefab {keyPrefabId}"));
                return false;
            }

            GameObject key = Instantiate(keyPrefab, keyHolder);
            key.transform.localPosition = new(0f, 0.02f, 0f);
            key.transform.localEulerAngles = new(0f, -90f, -90f);
            key.transform.localScale = new(1.2f, 1.2f, 1.2f);
            _keys.Add(key);
            _keysIds[keyHolderId] = keyPrefabId;
            _keysNames.Add(itemName);
            return true;
        }

        private void ApplyKeys()
        {
            if (IsCorrectKeys())
            {
                _hud.HideWidget(_hudConfig.HUDResetWidgetId);
                _soundsManager.PlaySound(_chestOpenSound, volume: _chestOpenSoundVolume);
                _inventory.RemoveItemByName(_noteItemName);
                _actionHandlers[_actionOpenName].MakeTransition(0);
                _returnToInventory = false;
                _maskCollider.enabled = true;
                StartCoroutine(OpenChestAnimationRoutine());
            }
            else
            {
                ReplicaManager.ShowReplica(_incorrectKeysRemark);
                ResetKeys();
            }
        }

        private IEnumerator OpenChestAnimationRoutine()
        {
            foreach (GameObject key in _keys)
            {
                MoveElement(key.transform, Vector3.zero, KEYS_ROTATION_OFFSET, _keysMovingTime);
            }

            yield return new WaitForSeconds(_keysMovingTime);
            _activator.DeactivateMiniGame();
            MoveElement(_chestCup, Vector3.zero, CHEST_CUP_OPEN_ROTATION_OFFSET, _openChestMovingTime);
        }

        private void ResetKeysIds()
        {
            for (int i = 0; i < _keysIds.Count; i++)
            {
                _keysIds[i] = -1;
            }
        }

        private void ResetKeys()
        {
            ResetKeysIds();

            foreach (GameObject key in _keys)
            {
                Destroy(key);
            }

            _keys.Clear();

            if (_returnToInventory)
            {
                foreach (string itemName in _keysNames)
                {
                    _inventory.AddItemByName(itemName);
                }
            }

            _keysNames.Clear();
            _dataToSave.keysData.Clear();
        }

        private bool IsCorrectKeys()
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keysIds[i] != _correctKeysIds[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsAllKeysInside() => !_keysIds.Contains(-1);

        private void OnActivated(bool isActivated)
        {
            foreach (Collider keyHolder in _keysHolders)
            {
                keyHolder.enabled = isActivated;
            }

            if (!isActivated && _actionHandlers[_actionOpenName].CurrentState != QuestActionState.Completed)
            {
                ResetKeys();
            }
        }

        private IEnumerator ClueRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_timeBeforeClue);

                if (_activator.IsActivated)
                {
                    ReplicaManager.ShowReplica(_puzzleRemark);
                }
            }
        }

        private void OnEnable()
        {
            _activator.Activated += OnActivated;
            _actionHandlers[_actionOpenName].StateChanged += OnStateChanged;
            _inputHandler.Reset += ResetKeys;
        }

        private void OnDisable()
        {
            _activator.Activated -= OnActivated;
            _actionHandlers[_actionOpenName].StateChanged -= OnStateChanged;
            _inputHandler.Reset -= ResetKeys;
        }

        private void OnStateChanged()
        {
            if (_actionHandlers[_actionOpenName].CurrentState == QuestActionState.Completed)
            {
                _progressManager.SaveAdditionalData(this);
            }
        }
    }
}