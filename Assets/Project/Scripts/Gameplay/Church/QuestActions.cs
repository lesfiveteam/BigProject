using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.HUD;
using BigProject.Systems.Inventory;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using BigProject.Utilities;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Localization;

namespace BigProject.Gameplay.Church
{
    public class QuestActions : MonoBehaviour
    {
        private InventorySystem _inventory;
        private SkinnedMeshRenderer _playerRenderer;
        private Collider _playerCollider;
        private RunesDriver _runesDriver;
        private GameplayManager _gameplayManager;
        private HUD _hud;
        private InventoryUI _inventoryUI;

        [SerializeField]
        private string _noteOne, _noteThree, _noteFour, _noteToGive;

        [SerializeField]
        private int _questId;


        [SerializeField]
        private GameObject _priest;
        [SerializeField]
        private GameObject _priestFinishPosition;
        [SerializeField]
        private CinemachineCamera _puzzleCamera;
        [SerializeField]
        private HUDConfig _hudConfig;
        [SerializeField]
        private GameObject _exit;
        [SerializeField]
        private QuestActionHandlerMono _getRunesAction;

        [Header("Player remarks")]
        [SerializeField]
        private LocalizedString _firstEnterRemark;
        [SerializeField]
        private LocalizedString _touchStillageRemark;
        [SerializeField]
        private LocalizedString _findNoteRemark;
        [SerializeField]
        private LocalizedString _getRunesRemark;

        public void Init(InventorySystem inventory, RuneShardsSystem runesSystem, RunesConfig runesConfig, RunePanelUI runePanel,
            Collider playerCollider, SkinnedMeshRenderer playerRenderer, GameplayManager gameplayManager, HUD hud, InventoryUI inventoryUI)
        {
            _inventory = inventory;
            _playerRenderer = playerRenderer;
            _playerCollider = playerCollider;
            _gameplayManager = gameplayManager;
            _hud = hud;
            _inventoryUI = inventoryUI;
            ExceptionUtilities.ThrowIfNull(_inventory, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Inventory System"));
            ExceptionUtilities.ThrowIfNull(_playerRenderer, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Collider"));
            ExceptionUtilities.ThrowIfNull(_playerCollider, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player Renderer"));
            ExceptionUtilities.ThrowIfNull(_hud, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "HUD"));
            ExceptionUtilities.ThrowIfNull(_inventoryUI, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "InventoryUI"));
            _runesDriver = new(runesSystem, runesConfig, runePanel, _questId, _priest.transform);
        }

        public void AddNoteOne()
        {
            _inventory.AddItemByName(_noteOne);
            ReplicaManager.ShowReplica(_findNoteRemark);
        }

        public void AddNoteFour()
        {
            _inventory.RemoveItemByName(_noteThree);
            _inventory.AddItemByName(_noteFour);
        }

        public void TouchStillageAction()
        {
            ReplicaManager.ShowReplica(_touchStillageRemark); // При начале обыске стеллажа
        }

        public void MovePriestToFinishPosition()
        {
            _priest.transform.position = _priestFinishPosition.transform.position;
        }

        public void DeactivatePuzzleCamera()
        {
            _puzzleCamera.enabled = false;
            _inventoryUI.SetNoteVisibility(false);
        }

        public void ActivatePlayer()
        {
            _playerRenderer.enabled = true;
            _playerCollider.enabled = true;
        }

        public void CloseExit()
        {
            _exit.SetActive(false);
        }

        public void GetRunes()
        {
            _inventory.RemoveItemByName(_noteFour);
            _inventory.AddItemByName(_noteToGive);
            _inventory.RemoveItemByName(_noteToGive);
            StartCoroutine(GetRunesRoutine());
            _getRunesAction.MakeTransition(0);
        }

        private IEnumerator GetRunesRoutine()
        {
            yield return new WaitUntil(() => _gameplayManager.State == GameplayState.Play);
            _hud.ShowWidget(_hudConfig.HUDRunesWidgetId);
            _runesDriver.Deliver();
            ReplicaManager.ShowReplica(_getRunesRemark);
        }
    }
}
